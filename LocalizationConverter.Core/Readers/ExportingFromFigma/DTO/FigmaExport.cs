using FluentResults;
using LocalizationConverter.Core.Constants;
using System.Text.Json;

namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

public record FigmaExport
{
    public static Result<FigmaExport> LoadFromFile(string path)
    {
        if (!File.Exists(path))
            return Result.Fail(Errors.FileNotFound);

        FigmaExport? figmaData;
        try
        {
            var jsonContent = File.ReadAllText(path);
            figmaData = JsonSerializer.Deserialize<FigmaExport>(jsonContent, options: new() { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return Result.Fail(Errors.FileReadingError);
        }

        return figmaData ?? Result.Fail<FigmaExport>(Errors.FileReadingError);
    }

    public Dictionary<string, FigmaCollection> Collections { get; set; } = [];
}
