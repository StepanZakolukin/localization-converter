namespace LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;

internal record FigmaVariables
{
    public List<FigmaStringVariable> Strings { get; set; } = [];
}
