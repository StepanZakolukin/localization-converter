using FluentResults;
using LocalizationConverter.Core.Constants;
using LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;
using System.Text.Json;

namespace LocalizationConverter.Core.Readers.ExportingFromFigma;

public static class FigmaVariableExportReader
{
    public static Result<FigmaExport> LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
            return Result.Fail(Errors.FileNotFound);

        FigmaExport? figmaData;
        try
        {
            var jsonContent = File.ReadAllText(filePath);
            figmaData = JsonSerializer.Deserialize<FigmaExport>(jsonContent, options: new() { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return Result.Fail(Errors.FileReadingError);
        }

        return figmaData ?? (Result<FigmaExport>)Result.Fail(Errors.FileReadingError);
    }
}
