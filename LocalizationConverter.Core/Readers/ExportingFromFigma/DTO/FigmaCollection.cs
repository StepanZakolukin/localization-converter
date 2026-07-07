namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

internal record FigmaCollection
{
    public FigmaVariables Variables { get; set; } = new();
}
