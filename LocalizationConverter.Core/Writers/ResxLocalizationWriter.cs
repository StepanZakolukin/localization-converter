using FluentResults;
using LocalizationConverter.Core.Constants;
using LocalizationConverter.Core.Models;
using System.Xml.Linq;

namespace LocalizationConverter.Core.Writers;

public class ResxLocalizationWriter(string directoryPath) : ILocalizationWriter
{
    public Result Write(LocalizationDirectory directory)
    {
        if (!Directory.Exists(directoryPath))
            return Result.Fail(Errors.FileNotFound);

        Write(directory, directoryPath);

        return Result.Ok();
    }

    private void Write(LocalizationDirectory directory, string dirPath)
    {
        foreach (var file in directory.FileList)
            GenerateResxFile(file, dirPath);

        foreach (var dir in directory.DirectoryList)
        {
            var currentPath = Path.Combine(dirPath, dir.Name);
            Directory.CreateDirectory(currentPath);
            Write(dir, currentPath);
        }
    }

    // TODO: надо рефакторить, возможно имеет смысл вынести в отдельный класс
    private void GenerateResxFile(LocalizationFile file, string targetDirectoryPath)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectoryPath);

        // 1. Формируем полный путь к конечному файлу ресурса
        var targetPath = Path.Combine(targetDirectoryPath, $"{file.Name}{FileExtensions.Resx}");

        // 2. Создаем корневой элемент со стандартными заголовками Microsoft
        var root = new XElement(ResxConstants.Nodes.Root,
            new XElement(
                ResxConstants.Nodes.Header,
                new XAttribute(ResxConstants.Nodes.NameAttribute, ResxConstants.Keys.MimeType),
                new XElement(ResxConstants.Nodes.Value, ResxConstants.Values.MimeType)
                ),
            new XElement(
                ResxConstants.Nodes.Header,
                new XAttribute(ResxConstants.Nodes.NameAttribute,
                ResxConstants.Keys.Version),
                new XElement(ResxConstants.Nodes.Value, ResxConstants.Values.Version)
                ),
            new XElement(
                ResxConstants.Nodes.Header,
                new XAttribute(ResxConstants.Nodes.NameAttribute, ResxConstants.Keys.Reader),
                new XElement(ResxConstants.Nodes.Value, ResxConstants.Values.ReaderType)
                ),
            new XElement(
                ResxConstants.Nodes.Header,
                new XAttribute(ResxConstants.Nodes.NameAttribute,
                ResxConstants.Keys.Writer),
                new XElement(ResxConstants.Nodes.Value, ResxConstants.Values.WriterType)
                )
        );

        // 3. Наполняем XML данными из коллекции переводов
        foreach (var localization in file.LocalizationList)
        {
            if (string.IsNullOrWhiteSpace(localization.Key))
                continue;

            var dataElement = new XElement(ResxConstants.Nodes.Data,
                new XAttribute(ResxConstants.Nodes.NameAttribute, localization.Key),
                new XAttribute(ResxConstants.Nodes.SpaceAttribute, ResxConstants.Nodes.PreserveValue),
                new XElement(ResxConstants.Nodes.Value, localization.Value ?? string.Empty)
            );

            root.Add(dataElement);
        }

        // 4. Записываем готовый XML-документ на диск
        var resxDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        resxDoc.Save(targetPath);
    }
}