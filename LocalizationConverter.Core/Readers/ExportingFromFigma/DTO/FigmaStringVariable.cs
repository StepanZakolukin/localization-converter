namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

internal record FigmaStringVariable
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
