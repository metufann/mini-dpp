namespace MiniDpp.Api.DTOs;

public class ImportResultDto
{
    public int TotalRows { get; set; }

    /// <summary>Rows that created or updated a product (CreatedRows + UpdatedRows).</summary>
    public int SavedRows { get; set; }

    public int CreatedRows { get; set; }
    public int UpdatedRows { get; set; }

    /// <summary>Valid rows identical to the stored product; no new version was created.</summary>
    public int UnchangedRows { get; set; }

    public int FailedRows { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = [];
}

public class ImportRowErrorDto
{
    public int Row { get; set; }
    public List<string> Reasons { get; set; } = [];
}
