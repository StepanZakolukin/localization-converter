namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

public record FigmaExport
{
    public Dictionary<string, FigmaCollection> Collections { get; set; } = [];
}
