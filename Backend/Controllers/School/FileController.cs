using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly mangeFilesService _mangeFilesService;

    public FileController(mangeFilesService mangeFilesService)
    {
        _mangeFilesService = mangeFilesService;
    }

    [HttpPost("uploadImage")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] int studentId, int voucherId = 0)
    {
        if (file == null)
            return BadRequest("No files uploaded.");

        try
        {
            var filePaths = await _mangeFilesService.UploadImage(file, studentId, voucherId);
            return Ok(new { success = true, filePaths });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    [HttpPost("uploadAttachments")]
    public async Task<IActionResult> UploadAttachments([FromForm] List<IFormFile> files, [FromForm] int studentId,[FromForm] int voucherId = 0)
    {
        if (files == null || !files.Any())
            return BadRequest("No files uploaded.");
        try
        {
            var filePaths = await _mangeFilesService.UploadAttachments(files, studentId, voucherId);
            return Ok(new { success = true, filePaths });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
