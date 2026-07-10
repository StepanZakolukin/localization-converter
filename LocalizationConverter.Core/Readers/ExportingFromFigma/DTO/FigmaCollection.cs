namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

public record FigmaCollection
{
    public FigmaVariables Variables { get; set; } = new();
}
