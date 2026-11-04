using Microsoft.AspNetCore.Mvc;
using MiniDpp.Api.DTOs;
using MiniDpp.Api.Services;

namespace MiniDpp.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController(CsvImportService importService) : ControllerBase
{
    public const long MaxUploadBytes = 2 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<ActionResult<ImportResultDto>> Import([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        var fileError = ValidateFile(file);
        if (fileError is not null)
        {
            return BadRequest(new { message = fileError });
        }

        try
        {
            await using var stream = file!.OpenReadStream();
            var result = await importService.ImportAsync(stream, cancellationToken);
            return Ok(result);
        }
        catch (CsvFileException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static string? ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return "A non-empty CSV file is required.";
        }

        if (file.Length > MaxUploadBytes)
        {
            return $"The CSV file exceeds the {MaxUploadBytes / (1024 * 1024)} MB upload limit.";
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.IsNullOrEmpty(file.FileName)
            && !string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            return "Only CSV files are accepted.";
        }

        return null;
    }
}
