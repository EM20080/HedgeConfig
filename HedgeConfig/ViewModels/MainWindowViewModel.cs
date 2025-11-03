using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HedgeConfig.Models;

namespace HedgeConfig.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string title = "HedgeConfig";
    [ObservableProperty] private bool isLightTheme = true;
    [ObservableProperty] private bool loadRootFolder = false;
    [ObservableProperty] private string? selectedModIniPath;

    public ObservableCollection<Option> Options { get; } = new();
    public ObservableCollection<JsonEnumEntry> CurrentEnumChoices { get; } = new();

    public event EventHandler? OpenConfigRequested;
    public event EventHandler? SelectModIniRequested;
    public event EventHandler? ExportConfigRequested;
    public event Func<string, Task<string?>>? RenameOptionRequested;
    public event Func<string, Task<string?>>? RenameChoiceRequested;

    private int _selectedOptionIndex = -1;
    public int SelectedOptionIndex
    {
        get => _selectedOptionIndex;
        set 
        { 
            SetProperty(ref _selectedOptionIndex, value); 
            OnPropertyChanged(nameof(HasSelectedOption));
            RefreshChoiceBindings(); 
        }
    }

    public bool HasSelectedOption => _selectedOptionIndex >= 0 && _selectedOptionIndex < Options.Count;

    private int _selectedChoiceIndex = -1;
    public int SelectedChoiceIndex
    {
        get => _selectedChoiceIndex;
        set 
        { 
            SetProperty(ref _selectedChoiceIndex, value);
            OnPropertyChanged(nameof(HasSelectedChoice));
            OnPropertyChanged(nameof(SelectedChoiceFolderName));
            OnPropertyChanged(nameof(SelectedChoiceDescription));
        }
    }

    public bool HasSelectedChoice => _selectedChoiceIndex >= 0 && _selectedChoiceIndex < CurrentEnumChoices.Count;

    public string SelectedChoiceFolderName
    {
        get
        {
            if (!HasSelectedOption || !HasSelectedChoice) return string.Empty;
            var option = Options[SelectedOptionIndex];
            if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < option.Choices.Count)
                return option.Choices[SelectedChoiceIndex].Folder;
            return string.Empty;
        }
        set
        {
            if (!HasSelectedOption || !HasSelectedChoice) return;
            var option = Options[SelectedOptionIndex];
            if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < option.Choices.Count)
            {
                option.Choices[SelectedChoiceIndex].Folder = value;
                OnPropertyChanged(nameof(SelectedChoiceFolderName));
                RefreshChoiceBindings();
            }
        }
    }

    public string SelectedOptionDescription
    {
        get
        {
            if (!HasSelectedOption) return string.Empty;
            return Options[SelectedOptionIndex].Description;
        }
        set
        {
            if (!HasSelectedOption) return;
            Options[SelectedOptionIndex].Description = value;
            OnPropertyChanged(nameof(SelectedOptionDescription));
        }
    }

    public string SelectedChoiceDescription
    {
        get
        {
            if (!HasSelectedOption || !HasSelectedChoice) return string.Empty;
            var option = Options[SelectedOptionIndex];
            if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < option.Choices.Count)
                return option.Choices[SelectedChoiceIndex].Description;
            return string.Empty;
        }
        set
        {
            if (!HasSelectedOption || !HasSelectedChoice) return;
            var option = Options[SelectedOptionIndex];
            if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < option.Choices.Count)
            {
                option.Choices[SelectedChoiceIndex].Description = value;
                OnPropertyChanged(nameof(SelectedChoiceDescription));
                RefreshChoiceBindings();
            }
        }
    }

    [RelayCommand]
    private void ToggleTheme() => IsLightTheme = !IsLightTheme;

    [RelayCommand]
    private void AddOption()
    {
        var baseName = "New Setting";
        var name = baseName;
        int i = 1;
        while (Options.Any(o => o.Name == name)) name = $"{baseName} {i++}";
        Options.Add(new Option { Name = name });
        SelectedOptionIndex = Options.Count - 1;
    }

    [RelayCommand]
    private void SelectOption(Option option)
    {
        var index = Options.IndexOf(option);
        if (index >= 0) SelectedOptionIndex = index;
    }

    [RelayCommand]
    private void DeleteOption()
    {
        if (SelectedOptionIndex < 0 || SelectedOptionIndex >= Options.Count) return;
        Options.RemoveAt(SelectedOptionIndex);
        SelectedOptionIndex = -1;
        OnPropertyChanged(nameof(HasSelectedOption));
    }

    [RelayCommand]
    private async Task RenameOptionAsync()
    {
        if (SelectedOptionIndex < 0 || SelectedOptionIndex >= Options.Count) return;
        var current = Options[SelectedOptionIndex];
        if (RenameOptionRequested == null) return;
        var newName = await RenameOptionRequested.Invoke(current.Name);
        if (!string.IsNullOrWhiteSpace(newName) && newName != current.Name)
        {
            current.Name = newName;
            if (current.OriginalJsonElement != null)
            {
                current.OriginalJsonElement.DisplayName = newName;
                current.OriginalJsonElement.Type = newName.Replace(" ", "");
                if (current.OriginalJsonElement.EnumName != null)
                    current.OriginalJsonElement.EnumName = newName.Replace(" ", "");
            }
        }
    }

    [RelayCommand]
    private void OpenConfig() => OpenConfigRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void SelectModIni() => SelectModIniRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ExportConfig() => ExportConfigRequested?.Invoke(this, EventArgs.Empty);

    public RootObject BuildConfigForExport()
    {
        var root = new RootObject
        {
            IniFile = "mod.ini",
            Groups = new List<ConfigGroup>(),
            Enums = new Dictionary<string, List<JsonEnumEntry>>()
        };

        var elements = new List<ConfigElement>();
        for (int i = 0; i < Options.Count; i++)
        {
            var option = Options[i];
            var element = option.OriginalJsonElement ?? new ConfigElement
            {
                Name = $"IncludeDir{i}",
                DisplayName = option.Name,
                Description = new List<string>(),
                Type = option.Name.Replace(" ", ""),
                EnumName = option.Name.Replace(" ", "")
            };
            element.Name = $"IncludeDir{i}";
            element.DisplayName = option.Name;
            element.Type = option.Name.Replace(" ", "");
            
            // Handle description
            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                element.Description = option.Description.Split('\n', StringSplitOptions.TrimEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();
            }
            else
            {
                element.Description = new List<string> { $"Auto-generated option for {option.Name}" };
            }
            
            if (option.Choices.Any())
            {
                string enumKey = option.Name.Replace(" ", "");
                element.EnumName = enumKey;
                var firstChoice = option.Choices.First();
                string defaultValue = string.IsNullOrEmpty(firstChoice.Folder) ? "./" : $"./{firstChoice.Folder.Replace("\\", "/")}/";
                if (!defaultValue.EndsWith("/")) defaultValue += "/";
                element.DefaultValue = defaultValue;
                var enumValues = new List<JsonEnumEntry>();
                foreach (var choice in option.Choices)
                {
                    string folderValue = string.IsNullOrEmpty(choice.Folder) ? "./" : $"./{choice.Folder.Replace("\\", "/")}/";
                    if (!folderValue.EndsWith("/")) folderValue += "/";
                    
                    List<string> choiceDescriptionList;
                    if (!string.IsNullOrWhiteSpace(choice.Description))
                    {
                        choiceDescriptionList = choice.Description.Split('\n', StringSplitOptions.TrimEntries)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();
                        if (choiceDescriptionList.Count == 0)
                            choiceDescriptionList = new List<string> { "" };
                    }
                    else
                    {
                        choiceDescriptionList = new List<string> { "" };
                    }
                    
                    enumValues.Add(new JsonEnumEntry
                    {
                        DisplayName = choice.Name,
                        Value = folderValue,
                        Description = choiceDescriptionList
                    });
                }
                root.Enums[enumKey] = enumValues;
            }
            else
            {
                element.EnumName = null;
                element.DefaultValue = "./";
            }
            elements.Add(element);
        }
        root.Groups.Add(new ConfigGroup { Name = "Main", DisplayName = "Options", Elements = elements });
        return root;
    }

    private void RefreshChoiceBindings()
    {
        CurrentEnumChoices.Clear();
        if (SelectedOptionIndex >= 0 && SelectedOptionIndex < Options.Count)
        {
            foreach (var c in Options[SelectedOptionIndex].Choices)
                CurrentEnumChoices.Add(new JsonEnumEntry{ DisplayName = c.Name, Value = c.Folder, Description = new(){""} });
            
            // Selects first choice the second you import a config
            if (CurrentEnumChoices.Count > 0 && SelectedChoiceIndex < 0)
            {
                SelectedChoiceIndex = 0;
            }
            else if (SelectedChoiceIndex >= CurrentEnumChoices.Count)
            {
                SelectedChoiceIndex = CurrentEnumChoices.Count > 0 ? 0 : -1;
            }
        }
        else
        {
            SelectedChoiceIndex = -1;
        }
        OnPropertyChanged(nameof(SelectedOptionDescription));
        OnPropertyChanged(nameof(SelectedChoiceDescription));
    }

    public void LoadConfig(RootObject config)
    {
        Options.Clear();
        SelectedOptionIndex = -1;
        SelectedModIniPath = config.IniFile;
        if (config.Groups != null && config.Groups.Count > 0)
        {
            foreach (var group in config.Groups)
            {
                if (group.Elements == null) continue;
                foreach (var element in group.Elements)
                {
                    var option = new Option
                    {
                        Name = element.DisplayName ?? element.Name,
                        Description = element.Description != null && element.Description.Count > 0 
                            ? string.Join("\n", element.Description) 
                            : string.Empty,
                        Choices = new List<Choice>(),
                        OriginalJsonElement = element
                    };
                    string enumKey = element.EnumName ?? element.Type ?? "";
                    if (!string.IsNullOrEmpty(enumKey) && config.Enums.ContainsKey(enumKey))
                    {
                        var enumValues = config.Enums[enumKey];
                        foreach (var value in enumValues)
                        {
                            string folder = value.Value ?? "";
                            if (folder.StartsWith("./")) folder = folder.Substring(2);
                            if (folder.EndsWith("/")) folder = folder.Substring(0, folder.Length - 1);
                            
                            string choiceDescription = value.Description != null && value.Description.Count > 0 
                                ? string.Join("\n", value.Description) 
                                : string.Empty;
                            
                            option.Choices.Add(new Choice 
                            { 
                                Name = value.DisplayName, 
                                Folder = folder,
                                Description = choiceDescription
                            });
                        }
                    }
                    Options.Add(option);
                }
            }
        }
    }

    [RelayCommand]
    private async Task RenameChoiceAsync()
    {
        if (!HasSelectedOption || !HasSelectedChoice) return;
        var currentOption = Options[SelectedOptionIndex];
        if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < currentOption.Choices.Count)
        {
            var choice = currentOption.Choices[SelectedChoiceIndex];
            if (RenameChoiceRequested == null) return;
            var newName = await RenameChoiceRequested.Invoke(choice.Name);
            if (!string.IsNullOrWhiteSpace(newName) && newName != choice.Name)
            {
                choice.Name = newName;
                RefreshChoiceBindings();
            }
        }
    }

    [RelayCommand]
    private void AddChoice()
    {
        if (!HasSelectedOption) return;
        var currentOption = Options[SelectedOptionIndex];
        var baseName = "New Choice";
        var name = baseName;
        int i = 1;
        while (currentOption.Choices.Any(c => c.Name == name)) name = $"{baseName} {i++}";
        currentOption.Choices.Add(new Choice { Name = name, Folder = $"folder{currentOption.Choices.Count + 1}" });
        RefreshChoiceBindings();
        SelectedChoiceIndex = CurrentEnumChoices.Count - 1;
    }

    [RelayCommand]
    private void DeleteChoice()
    {
        if (!HasSelectedOption || !HasSelectedChoice) return;
        var currentOption = Options[SelectedOptionIndex];
        if (SelectedChoiceIndex >= 0 && SelectedChoiceIndex < currentOption.Choices.Count)
        {
            currentOption.Choices.RemoveAt(SelectedChoiceIndex);
            RefreshChoiceBindings();
            SelectedChoiceIndex = -1;
        }
    }
}
