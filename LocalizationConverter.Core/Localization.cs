namespace LocalizationConverter.Core;

public record Localization
{
    public required string Key { get; init;  }
    public required string Value { get; init; }
}
