namespace LocalizationConverter.Core.Models;

public class LocalizationFile
{
    public required string Name { get; init; }
    public IReadOnlyCollection<Localization> LocalizationList => _localizationDictionary.Values;

    private readonly Dictionary<string, Localization> _localizationDictionary = new(StringComparer.OrdinalIgnoreCase);
    public bool TryAddLocalization(Localization localization) => _localizationDictionary.TryAdd(localization.Key, localization);
}
