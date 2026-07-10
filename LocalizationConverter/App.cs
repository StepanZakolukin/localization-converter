using LocalizationConverter.Core.Constants;
using LocalizationConverter.Core.Converters;
using LocalizationConverter.Core.Readers.ExportingFromFigma;
using LocalizationConverter.Core.Writers;
using Terminal.Gui;

namespace LocalizationConverter;

internal static class App
{
    public static void Run()
    {
        Application.Init();

        if (!TrySelectFile(out var selectedJsonPath))
        {
            Application.Shutdown();
            Console.WriteLine("Действие отменено пользователем.");
            return;
        }

        var selectedCollections = SelectCollections(selectedJsonPath!);
        if (selectedCollections.Count == 0)
        {
            Application.Shutdown();
            Console.WriteLine("Не выбрано ни одной коллекции для конвертации.");
            return;
        }
        if (!TrySelectFolder(out var selectedDirectoryPath))
        {
            Application.Shutdown();
            Console.WriteLine("Действие отменено пользователем.");
            return;
        }

        Application.Shutdown();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[Успешно собраны параметры для запуска]");
        Console.ResetColor();

        Console.WriteLine($"-> Путь к JSON: {selectedJsonPath}");
        Console.WriteLine($"-> Выбранные коллекции ({selectedCollections.Count}): {string.Join(", ", selectedCollections)}");
        Console.WriteLine($"-> Папка назначения: {selectedDirectoryPath}");

        Console.WriteLine("\nНачинаю конвертацию переменных Figma...");

        var reader = new ExportingFromFigmaLocalizationReader(selectedJsonPath!, selectedCollections);
        var writer = new ResxLocalizationWriter(selectedDirectoryPath!);
        var converter = new Converter(reader, writer);
        var conversionResult = converter.Convert();
        if (conversionResult.IsFailed)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Join(", ", conversionResult.Errors));
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Готово!");
        Console.ResetColor();
    }

    private static bool TrySelectFile(out string? filePath)
    {
        filePath = null;

        var fileDialog = new OpenDialog("Шаг 1: Выберите JSON с переменными Figma", "Выберите файл")
        {
            AllowedFileTypes = [FileExtensions.Json],
            AllowsMultipleSelection = false,
            CanChooseFiles = true,
            CanChooseDirectories = false,
            DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        Application.Run(fileDialog);

        if (fileDialog.Canceled)
            return false;

        filePath = fileDialog.FilePath.ToString();
        return true;
    }

    private static List<string> SelectCollections(string selectedJsonPath)
    {
        var selectedCollections = new List<string>();
        var loadResult = FigmaVariableExportReader.LoadFile(selectedJsonPath);

        //TODO: Доработать
        if (loadResult.IsSuccess)
        {
            MessageBox.ErrorQuery("Критическая ошибка", $"Не удалось загрузить файл: {string.Join(',', loadResult.Errors)}", "ОК");
        }

        var availableCollections = loadResult.Value.Collections.Keys.ToArray();
        if (availableCollections.Length == 0)
            MessageBox.ErrorQuery("Предупреждение", "Не удалось распарсить файл или файл не содержит коллекций.", "ОК");
        //
        var collectionsWindow = new Window("Шаг 2: Выбор коллекций для конвертации")
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Percent(60),
            Height = Dim.Percent(60)
        };

        var instructionLabel = new Label("Отметьте нужные коллекции пробелом или мышкой:") { X = 1, Y = 1 };

        var listView = new ListView(availableCollections)
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            AllowsMarking = true
        };

        var nextButton = new Button("Далее", is_default: true)
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd(1),
            Enabled = false
        };

        void UpdateNextButtonState()
        {
            var hasSelection = false;
            for (var i = 0; i < availableCollections.Length; i++)
            {
                if (listView.Source.IsMarked(i))
                {
                    hasSelection = true;
                    break;
                }
            }
            nextButton.Enabled = hasSelection;
        }

        // 3. Подписываемся на события ввода, чтобы проверять галки на лету
        listView.KeyPress += (_) => Application.MainLoop.Invoke(UpdateNextButtonState);
        listView.MouseClick += (_) => Application.MainLoop.Invoke(UpdateNextButtonState);

        nextButton.Clicked += () =>
        {
            for (var i = 0; i < availableCollections.Length; i++)
            {
                if (listView.Source.IsMarked(i))
                    selectedCollections.Add(availableCollections[i]);
            }

            Application.RequestStop();
        };

        collectionsWindow.Add(instructionLabel, listView, nextButton);
        Application.Run(collectionsWindow);

        return selectedCollections;
    }

    private static bool TrySelectFolder(out string? folderPath)
    {
        folderPath = null;

        var folderDialog = new OpenDialog("Шаг 3: Выберите папку для сохранения ресурсов", "Выбрать папку")
        {
            AllowsMultipleSelection = false,
            CanChooseFiles = false,
            CanChooseDirectories = true,
            DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        Application.Run(folderDialog);

        if (folderDialog.Canceled)
            return false;

        folderPath = $"{folderDialog.FilePath}";
        return true;
    }
}
