namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

internal record FigmaExport
{
    public Dictionary<string, FigmaCollection> Collections { get; set; } = [];
}
