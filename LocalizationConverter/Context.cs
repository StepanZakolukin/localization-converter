namespace LocalizationConverter;

// Контекст для хранения данных между шагами
public class Context
{
    public string InputFilePath { get; set; } = string.Empty;
    public List<string> SelectedCollections { get; set; } = [];
    public string OutputFilePath { get; set; } = string.Empty;
}

