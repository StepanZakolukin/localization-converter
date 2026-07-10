using FluentResults;
using LocalizationConverter.Core.Constants;
using LocalizationConverter.Core.Models;
using LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

namespace LocalizationConverter.Core.Readers.ExportingFromFigma;

public class ExportingFromFigmaLocalizationReader(string filePath, IEnumerable<string> collectionNames) : ILocalizationReader
{
    public Result<LocalizationDirectory> Read()
    {
        var fileLoadResult = FigmaExport.LoadFromFile(filePath);
        if (fileLoadResult.IsFailed)
            return fileLoadResult.ToResult();

        var data = fileLoadResult.Value;

        //  папка верхнего уровня, нужна исключительно как обертка и не будет использоваться при создании файловой структуры файлов локализации
        var root = new LocalizationDirectory() { Name = "root" };

        foreach (var collectionName in collectionNames)
        {
            if (!data.Collections.TryGetValue(collectionName, out var collection))
                return Result.Fail(Errors.CollectionNotFound);

            foreach (var variable in collection.Variables.Strings)
            {
                var nesting = variable.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (nesting.Length < 2)
                {
                    return Result.Fail($"Некорректное именование переменной {variable.Name}. " +
                        $"Именование должно содержать название файла в который следует записать переменную (Пример: 'ИмяФайла/ИмяПеременной').");
                }

                var localization = new Localization() { Key = nesting[^1], Value = variable.Value };

                var file = root.GetOrCreateFile(nesting[..^1]);
                if (!file.TryAddLocalization(localization))
                    return Result.Fail(Errors.LocalizationKeyIsDuplicated);
            }
        }

        return Result.Ok(root);
    }
}
