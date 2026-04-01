namespace Backend.DTOS.School.DatabaseRestore;

/// <summary>Result of import-only .bak flow: data merged into the existing SqlAdminConnection database.</summary>
public class DatabaseImportResultDTO
{
    public string TargetDatabaseName { get; set; } = "";
    /// <summary>Temp DB name used during import; it is dropped when the operation finishes.</summary>
    public string TemporaryDatabaseName { get; set; } = "";
    public bool TemporaryDatabaseDropped { get; set; }
    public int TablesImported { get; set; }
    public IReadOnlyList<string> ImportedTableNames { get; set; } = Array.Empty<string>();
    public string BackupPathOnServer { get; set; } = "";
}
