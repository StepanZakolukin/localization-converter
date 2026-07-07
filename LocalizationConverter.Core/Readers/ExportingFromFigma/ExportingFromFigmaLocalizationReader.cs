using FluentResults;
using LocalizationConverter.Core.Constants;
using LocalizationConverter.Core.Models;
using LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;
using System.Text.Json;

namespace LocalizationConverter.Core.Readers.ExportingFromFigma;

internal class ExportingFromFigmaLocalizationReader(string filePath, string collectionName) : ILocalizationReader
{
    public Result<LocalizationDirectory> Read()
    {
        if (!File.Exists(filePath))
            return Result.Fail(Errors.FileNotFound);

        FigmaExport? figmaData;
        try
        {
            var jsonContent = File.ReadAllText(filePath);
            figmaData = JsonSerializer.Deserialize<FigmaExport>(jsonContent);
        }
        catch
        {
            return Result.Fail(Errors.FileReadingError);
        }

        if (figmaData is null)
            return Result.Fail(Errors.FileReadingError);

        if (!figmaData.Collections.TryGetValue(collectionName, out var collection))
            return Result.Fail(Errors.CollectionNotFound);

        //  папка верхнего уровня, нужна исключительно как обертка и не будет использоваться при создании файловой структуры файлов локализации
        var root = new LocalizationDirectory() { Name = "root" };

        foreach (var variable in collection.Variables.Strings)
        {
            var nesting = variable.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var localization = new Localization() { Key = nesting[^1], Value = variable.Value };

            var file = root.GetOrCreateFile(nesting[..^1]);
            if (!file.TryAddLocalization(localization))
                return Result.Fail(Errors.LocalizationKeyIsDuplicated);
        }

        return Result.Ok(root);
    }
}
