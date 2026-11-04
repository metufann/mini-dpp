namespace MiniDpp.Api.DTOs;

public class ImportResultDto
{
    public int TotalRows { get; set; }
    public int SavedRows { get; set; }
    public int FailedRows { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = [];
}

public class ImportRowErrorDto
{
    public int Row { get; set; }
    public List<string> Reasons { get; set; } = [];
}
