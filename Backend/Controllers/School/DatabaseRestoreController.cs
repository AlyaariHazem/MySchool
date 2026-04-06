using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Security.Claims;

namespace Backend.Controllers.School;

/// <summary>Upload a .bak: restore to a temporary database, copy dbo data into the logged-in user's tenant database, then drop the temp DB.</summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "DatabaseRestore")]
public class DatabaseRestoreController : ControllerBase
{
    private readonly SqlRestoreService _sqlRestoreService;
    private readonly TenantInfo _tenantInfo;
    private readonly IAuditTrailService _auditTrail;
    private readonly ILogger<DatabaseRestoreController> _logger;

    public DatabaseRestoreController(
        SqlRestoreService sqlRestoreService,
        TenantInfo tenantInfo,
        IAuditTrailService auditTrail,
        ILogger<DatabaseRestoreController> logger)
    {
        _sqlRestoreService = sqlRestoreService;
        _tenantInfo = tenantInfo;
        _auditTrail = auditTrail;
        _logger = logger;
    }

    /// <param name="file">.bak backup file (schema should match the target database).</param>
    [HttpPost("restore")]
    [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
    [RequestSizeLimit(2_147_483_648)]
    [ProducesResponseType(typeof(APIResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(APIResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> RestoreFromBackup(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        var response = new APIResponse();

        if (file == null || file.Length == 0)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add("No .bak file was uploaded.");
            return BadRequest(response);
        }

        if (!file.FileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add("Only .bak backup files are accepted.");
            return BadRequest(response);
        }

        if (string.IsNullOrWhiteSpace(_tenantInfo.ConnectionString))
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(
                "No database is associated with this request. Log in as a user linked to a school (tenant), or as an administrator.");
            return BadRequest(response);
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _sqlRestoreService.ImportBackupIntoExistingDatabaseAsync(
                    stream,
                    file.FileName,
                    _tenantInfo.ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);

            await _auditTrail.RecordAsync(
                "Database",
                "Database.RestoreFromBackup",
                new
                {
                    BackupFileName = file.FileName,
                    result.TargetDatabaseName,
                    result.TemporaryDatabaseName,
                    result.TablesImported,
                    result.ImportedTableNames
                },
                cancellationToken);

            response.Result = result;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            _logger.LogError(
                ex,
                "AUDIT Database Database.RestoreFromBackup.Failed at {TimestampUtc:o} ActorUserId={ActorUserId} TenantId={TenantId} File={FileName}",
                DateTime.UtcNow,
                userId ?? "",
                _tenantInfo.TenantId,
                file?.FileName ?? "");
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(FormatRestoreError(ex));
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    private static string FormatRestoreError(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
        {
            if (e is SqlException sql)
            {
                var text = $"SQL error {sql.Number} (state {sql.State}, class {sql.Class}): {sql.Message}";
                if (sql.Number == 3169)
                {
                    text += " The .bak was produced by a newer SQL Server than the instance you are restoring to. "
                        + "Upgrade the restore target (e.g. Docker image mcr.microsoft.com/mssql/server:2025-latest if backups come from SQL Server 2025), "
                        + "or take a new backup from a server whose version is the same or older than the instance you restore on. "
                        + "If you use Docker, ensure ConnectionStrings point at the compose sqlserver service, not an older local SQL instance.";
                }

                return text;
            }
        }

        var parts = new List<string>();
        for (var e = ex; e != null; e = e.InnerException)
        {
            parts.Add(e.Message);
        }

        return string.Join(" — ", parts);
    }
}
