namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

public record FigmaVariables
{
    public List<FigmaStringVariable> Strings { get; set; } = [];
}
