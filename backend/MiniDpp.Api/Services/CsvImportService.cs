using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using MiniDpp.Api.Data;
using MiniDpp.Api.DTOs;
using MiniDpp.Api.Models;
using MiniDpp.Api.Validators;

namespace MiniDpp.Api.Services;

public class CsvImportService(AppDbContext db, ProductRowValidator rowValidator)
{
    public static readonly string[] RequiredHeaders =
    [
        "gtin",
        "model_adi",
        "batch_no",
        "elyaf_pamuk_yuzde",
        "elyaf_elastan_yuzde",
        "uretim_yeri",
        "karbon_kg_co2e"
    ];

    public async Task<ImportResultDto> ImportAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        if (csvStream.CanSeek && csvStream.Length == 0)
        {
            throw new CsvFileException("The CSV file is empty.");
        }

        var result = new ImportResultDto();
        var options = new CsvReaderOptions
        {
            HasHeader = true,
            Separator = ',',
            Encoding = Encoding.UTF8,
            FormatProvider = CultureInfo.InvariantCulture,
            TrimWhitespace = true,
            TrackLineNumbers = true
        };

        using var csv = CsvReader.Create(csvStream, options, leaveOpen: true);

        try
        {
            await csv.ReadHeaderAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw new CsvFileException("The CSV file could not be parsed.");
        }

        ValidateHeaders(csv.Headers);

        var seenGtins = new HashSet<string>(StringComparer.Ordinal);
        var existingGtins = (await db.Products.Select(product => product.Gtin).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        var modelsByName = await db.ProductModels.ToDictionaryAsync(
            model => model.Name,
            StringComparer.OrdinalIgnoreCase,
            cancellationToken);
        var batches = await db.Batches.ToListAsync(cancellationToken);

        while (await csv.ReadAsync(cancellationToken))
        {
            var lineNumber = csv.LineNumber > 0 ? (int)csv.LineNumber : result.TotalRows + 2;
            result.TotalRows++;

            CsvProductRow row;
            try
            {
                row = ReadRow(csv);
            }
            catch (Exception)
            {
                AddError(result, lineNumber, "The CSV record is malformed.");
                continue;
            }

            var validationErrors = rowValidator.Validate(row, out var validated).ToList();
            if (validated is null)
            {
                AddError(result, lineNumber, validationErrors);
                continue;
            }

            if (!seenGtins.Add(validated.Gtin) || existingGtins.Contains(validated.Gtin))
            {
                AddError(result, lineNumber, "A product with this GTIN already exists.");
                continue;
            }

            var model = GetOrCreateModel(modelsByName, validated.ModelName);
            var batch = GetOrCreateBatch(batches, model, validated.BatchNumber, validated.ProductionPlace);

            db.Products.Add(new Product
            {
                Gtin = validated.Gtin,
                Batch = batch,
                CottonPercentage = validated.CottonPercentage,
                ElastanePercentage = validated.ElastanePercentage,
                CarbonKgCO2e = validated.CarbonKgCO2e
            });

            existingGtins.Add(validated.Gtin);
            result.SavedRows++;
        }

        await db.SaveChangesAsync(cancellationToken);
        result.FailedRows = result.Errors.Count;
        return result;
    }

    private static CsvProductRow ReadRow(CsvReader csv) => new()
    {
        Gtin = GetField(csv, "gtin"),
        ModelName = GetField(csv, "model_adi"),
        BatchNumber = GetField(csv, "batch_no"),
        CottonPercentage = GetField(csv, "elyaf_pamuk_yuzde"),
        ElastanePercentage = GetField(csv, "elyaf_elastan_yuzde"),
        ProductionPlace = GetField(csv, "uretim_yeri"),
        CarbonKgCO2e = GetField(csv, "karbon_kg_co2e")
    };

    private static string? GetField(CsvReader csv, string name)
    {
        if (!csv.TryGetFieldIndex(name, out var index))
        {
            return null;
        }

        return csv.GetStringOrNull(index);
    }

    private static void ValidateHeaders(IEnumerable<string>? headers)
    {
        var present = headers?
            .Select(header => header.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (present is null || present.Count == 0)
        {
            throw new CsvFileException("The CSV header row is missing.");
        }

        var missing = RequiredHeaders.Where(header => !present.Contains(header)).ToList();
        if (missing.Count > 0)
        {
            throw new CsvFileException($"The CSV header is missing required columns: {string.Join(", ", missing)}.");
        }
    }

    private static ProductModel GetOrCreateModel(
        Dictionary<string, ProductModel> modelsByName,
        string name)
    {
        if (modelsByName.TryGetValue(name, out var existing))
        {
            return existing;
        }

        var model = new ProductModel { Name = name };
        modelsByName[name] = model;
        return model;
    }

    private static Batch GetOrCreateBatch(
        List<Batch> batches,
        ProductModel model,
        string batchNumber,
        string productionPlace)
    {
        var existing = batches.FirstOrDefault(batch =>
            batch.ProductModel == model
            && string.Equals(batch.BatchNumber, batchNumber, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing;
        }

        var batch = new Batch
        {
            BatchNumber = batchNumber,
            ProductionPlace = productionPlace,
            ProductModel = model
        };
        batches.Add(batch);
        return batch;
    }

    private static void AddError(ImportResultDto result, int row, string reason) =>
        AddError(result, row, [reason]);

    private static void AddError(ImportResultDto result, int row, IEnumerable<string> reasons)
    {
        result.Errors.Add(new ImportRowErrorDto
        {
            Row = row,
            Reasons = reasons.ToList()
        });
    }
}

public class CsvFileException(string message) : Exception(message);
