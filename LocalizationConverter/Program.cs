using LocalizationConverter.Core.Converters;
using LocalizationConverter.Core.Readers.ExportingFromFigma;
using LocalizationConverter.Core.Readers.ExportingFromFigma.DTO;
using LocalizationConverter.Core.Writers;
using Spectre.Console;

namespace LocalizationConverter;

internal static class Program
{
    private const string ExitCommand = "exit";
    private const string BackCommand = "back";

    private static void Main(string[] args)
    {
        var context = new Context();
        var currentStep = Step.InputPath;

        while (currentStep != Step.Exit)
        {
            Console.Clear();
            RenderHeader();

            currentStep = currentStep switch
            {
                Step.InputPath => RunInputPathStep(context),
                Step.SelectItems => RunSelectItemsStep(context),
                Step.OutputPath => RunOutputPathStep(context),
                Step.Summary => RunSummaryStep(context),
                _ => Step.Exit
            };
        }

        Console.Clear();
        RenderHeader();
        AnsiConsole.MarkupLine("[bold green]✔ Работа завершена успешно![/]");
    }

    private static void RenderHeader()
    {
        AnsiConsole.Write(
            new Panel(new Text(" Localization Converter ", new Style(Color.Green)).Centered())
                .BorderColor(Color.Green)
                .Expand());
        AnsiConsole.WriteLine();
    }

    // ШАГ 1: Выбор пути к исходному файлу
    private static Step RunInputPathStep(Context context)
    {
        AnsiConsole.Write(new Rule("[yellow]Шаг 1: Путь к исходному файлу[/]") { Justification = Justify.Left });

        var prompt = new TextPrompt<string>("Введите путь к [green]исходному[/] файлу (можно в кавычках или 'exit'):")
            .PromptStyle("white")
            .Validate(path =>
            {
                var cleanInput = path.Trim('"', ' ');

                return cleanInput.Equals(ExitCommand, StringComparison.OrdinalIgnoreCase)
                    ? ValidationResult.Success()
                    : FigmaExport.LoadFromFile(cleanInput).IsSuccess ? ValidationResult.Success() : ValidationResult.Error();
            });

        // Подставляем ранее введенный путь как значение по умолчанию
        if (!string.IsNullOrEmpty(context.InputFilePath))
            prompt.DefaultValue(context.InputFilePath);

        var rawPath = AnsiConsole.Prompt(prompt);
        var cleanPath = rawPath.Trim('"', ' ');

        if (cleanPath.Equals(ExitCommand, StringComparison.OrdinalIgnoreCase))
            return Step.Exit;

        context.InputFilePath = Path.GetFullPath(cleanPath);

        return Step.SelectItems;
    }

    // ШАГ 2: Выбор значений на основе данных файла
    private static Step RunSelectItemsStep(Context context)
    {
        AnsiConsole.Write(new Rule("[yellow]Шаг 2: Выбор параметров из файла[/]") { Justification = Justify.Left });
        AnsiConsole.MarkupLine($"[grey]Файл загружен: {context.InputFilePath}[/]\n");

        const string backOptionMarker = "__BACK_STEP__";

        // Формируем список для интерактивного выбора (добавляем опцию возврата)
        var choices = new List<string> { backOptionMarker };
        var figmaExport = FigmaExport.LoadFromFile(context.InputFilePath).Value;
        choices.AddRange(figmaExport.Collections.Keys);

        var prompt = new MultiSelectionPrompt<string>()
            .Title("Выберите нужные значения из списка ([green]Пробел[/] - выбор, [green]Enter[/] - подтвердить):")
            .Required()
            .PageSize(10)
            .MoreChoicesText("[grey](Листайте вверх/вниз для просмотра всех вариантов)[/]")
            .InstructionsText("[grey](Нажмите пробел для выбора элементов, затем Enter)[/]")
            .UseConverter(choice => choice == backOptionMarker
                ? "[bold red]◀ НАЗАД (Изменить исходный файл)[/]"
                : choice)
            .AddChoices(choices);

        // Восстанавливаем ранее выбранные галочки, если пользователь вернулся с Шага 3
        if (context.SelectedCollections != null && context.SelectedCollections.Count > 0)
        {
            foreach (var previousSelection in context.SelectedCollections)
                prompt.Select(previousSelection);
        }

        var selected = AnsiConsole.Prompt(prompt);

        // Если была выбрана кнопка "Назад"
        if (selected.Contains(backOptionMarker))
            return Step.InputPath;

        context.SelectedCollections = selected;
        return Step.OutputPath;
    }

    // ШАГ 3: Выбор пути до папки / файла сохранения
    private static Step RunOutputPathStep(Context context)
    {
        AnsiConsole.Write(new Rule("[yellow]Шаг 3: Сохранение результатов[/]") { Justification = Justify.Left });

        var prompt = new TextPrompt<string>("Укажите путь для [green]сохранения[/] файла (или напишите [bold red]'back'[/], чтобы вернуться назад):")
            .PromptStyle("white")
            .Validate(input =>
            {
                var cleanInput = input.Trim('"', ' ');
                if (cleanInput.Equals(BackCommand, StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Success();

                var directory = Path.GetDirectoryName(cleanInput);
                return string.IsNullOrEmpty(directory) || Directory.Exists(directory)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Указанная директория не существует.[/]");
            });

        if (!string.IsNullOrEmpty(context.OutputFilePath))
            prompt.DefaultValue(context.OutputFilePath);

        var rawPath = AnsiConsole.Prompt(prompt);
        var cleanPath = rawPath.Trim('"', ' ');

        if (cleanPath.Equals(BackCommand, StringComparison.OrdinalIgnoreCase))
            return Step.SelectItems;

        context.OutputFilePath = Path.GetFullPath(cleanPath);
        return Step.Summary;
    }

    // ШАГ 4: Итоговая обработка и финализация
    private static Step RunSummaryStep(Context context)
    {
        AnsiConsole.Write(new Rule("[yellow]Итоги операции[/]") { Justification = Justify.Left });

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold blue]Параметр[/]");
        table.AddColumn("[bold blue]Значение[/]");
        table.AddRow("Исходный файл", $"[green]{context.InputFilePath}[/]");
        table.AddRow("Выбрано коллекций", $"[yellow]{context.SelectedCollections.Count}[/]");
        table.AddRow("Путь сохранения", $"[green]{context.OutputFilePath}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (AnsiConsole.Confirm("Выполнить преобразование данных?"))
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("[bold yellow]Конвертация локализации...[/]", _ => Convert(context));

            AnsiConsole.MarkupLine("\n[green]✔ Преобразование завершено успешно![/]");
            AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для выхода...[/]");
            Console.ReadKey(true);
            return Step.Exit;
        }

        return Step.OutputPath;
    }

    private static void Convert(Context context)
    {
        var reader = new ExportingFromFigmaLocalizationReader(context.InputFilePath, context.SelectedCollections);
        var writer = new ResxLocalizationWriter(context.OutputFilePath);
        var converter = new Converter(reader, writer);
        converter.Convert();
    }
}
