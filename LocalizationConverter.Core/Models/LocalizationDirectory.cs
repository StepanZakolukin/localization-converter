namespace LocalizationConverter.Core.Models;

public class LocalizationDirectory
{
    public required string Name { get; init; }

    public IReadOnlyCollection<LocalizationDirectory> DirectoryList => _directoryDictionary.Values;
    private readonly Dictionary<string, LocalizationDirectory> _directoryDictionary = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<LocalizationFile> FileList => _fileDictionary.Values;
    private readonly Dictionary<string, LocalizationFile> _fileDictionary = new(StringComparer.OrdinalIgnoreCase);

    public LocalizationDirectory GetOrCreateDirectory(string name)
    {
        if (_directoryDictionary.TryGetValue(name, out var directory))
            return directory;

        directory = new LocalizationDirectory() { Name = name };
        _directoryDictionary.Add(name, directory);

        return directory;
    }

    public LocalizationDirectory GetOrCreateDirectory(params string[] nesting)
    {
        var currentDirectory = this;

        foreach(var lookupDirectoryName in nesting)
            currentDirectory = currentDirectory.GetOrCreateDirectory(lookupDirectoryName);

        return currentDirectory;
    }

    public LocalizationFile GetOrCreateFile(string name)
    {
        if (_fileDictionary.TryGetValue(name, out var file))
            return file;

        file = new LocalizationFile() { Name = name };
        _fileDictionary.Add(name, file);

        return file;
    }

    public LocalizationFile GetOrCreateFile(params string[] nesting)
    {
        var directory = GetOrCreateDirectory(nesting[..^1]);
        return directory.GetOrCreateFile(nesting[^1]);
    }
}
