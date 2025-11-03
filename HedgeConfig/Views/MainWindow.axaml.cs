using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using HedgeConfig.ViewModels;
using HedgeConfig.Models;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System;

namespace HedgeConfig.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        ApplyTheme();
        if (DataContext is MainWindowViewModel vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;
            vm.OpenConfigRequested += OnOpenConfigRequested;
            vm.SelectModIniRequested += OnSelectModIniRequested;
            vm.ExportConfigRequested += OnExportConfigRequested;
            vm.RenameOptionRequested += OnRenameOptionRequested;
            vm.RenameChoiceRequested += OnRenameChoiceRequested;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsLightTheme)) ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (DataContext is MainWindowViewModel vm)
            RequestedThemeVariant = vm.IsLightTheme ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    private async void OnOpenConfigRequested(object? sender, EventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Config File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (files.Count > 0 && DataContext is MainWindowViewModel vm)
        {
            try
            {
                var json = await File.ReadAllTextAsync(files[0].Path.LocalPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                var config = JsonSerializer.Deserialize<RootObject>(json, options);
                if (config != null) vm.LoadConfig(config);
            }
            catch { }
        }
    }

    private async void OnSelectModIniRequested(object? sender, EventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select mod.ini File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("INI Files") { Patterns = new[] { "*.ini" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (files.Count > 0 && DataContext is MainWindowViewModel vm)
            vm.SelectedModIniPath = files[0].Path.LocalPath;
    }

    private async void OnExportConfigRequested(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        
        if (vm.Options.Count == 0)
        {
            await ShowMessageDialog("Error", "Add at least one setting before exporting.");
            return;
        }
        
        if (string.IsNullOrEmpty(vm.SelectedModIniPath))
        {
            await ShowMessageDialog("Error", "Select a mod.ini file before exporting.");
            return;
        }
        
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Config",
            SuggestedFileName = "config.json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (file != null)
        {
            try
            {
                WriteModIni(vm);
                var root = vm.BuildConfigForExport();
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                };
                var json = JsonSerializer.Serialize(root, options);
                await File.WriteAllTextAsync(file.Path.LocalPath, json);
                await ShowMessageDialog("Success", "Config exported successfully!");
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Error", $"Error exporting: {ex.Message}");
            }
        }
    }

    private void WriteModIni(MainWindowViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.SelectedModIniPath)) return;
        
        var iniLines = new List<string>();
        if (File.Exists(vm.SelectedModIniPath))
        {
            var existingLines = File.ReadAllLines(vm.SelectedModIniPath);
            foreach (var line in existingLines)
            {
                if (!line.TrimStart().StartsWith("IncludeDir", StringComparison.OrdinalIgnoreCase) &&
                    !line.TrimStart().StartsWith("IncludeDirCount", StringComparison.OrdinalIgnoreCase))
                    iniLines.Add(line);
            }
        }

        if (vm.LoadRootFolder)
            iniLines.Add("IncludeDir0=\".\"");

        for (int i = 0; i < vm.Options.Count; i++)
        {
            int includeDirIndex = vm.LoadRootFolder ? i + 1 : i;
            string folder = vm.Options[i].Choices.Count > 0 ? vm.Options[i].Choices[0].Folder.Trim() : "";
            string includeValue;
            if (string.IsNullOrEmpty(folder))
            {
                includeValue = ".";
            }
            else
            {
                folder = folder.Replace("\\", "/");
                if (folder.StartsWith("./")) folder = folder[2..];
                if (!folder.EndsWith("/")) folder += "/";
                includeValue = "./" + folder;
            }
            iniLines.Add($"IncludeDir{includeDirIndex}=\"{includeValue}\"");
        }

        int includeDirCount = vm.LoadRootFolder ? vm.Options.Count + 1 : vm.Options.Count;
        iniLines.Add($"IncludeDirCount={includeDirCount}");
        File.WriteAllLines(vm.SelectedModIniPath, iniLines);
    }

    private async Task ShowMessageDialog(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            RequestedThemeVariant = RequestedThemeVariant
        };

        var panel = new StackPanel { Margin = new Thickness(16) };
        var label = new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 16), TextWrapping = global::Avalonia.Media.TextWrapping.Wrap };
        var okButton = new Button { Content = "OK", Width = 80, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center };
        okButton.Click += (s, e) => dialog.Close();
        panel.Children.Add(label);
        panel.Children.Add(okButton);
        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }

    private Task<string?> OnRenameOptionRequested(string currentName) => ShowInputDialog("Rename Setting", "Enter new setting name:", currentName);

    private Task<string?> OnRenameChoiceRequested(string currentName) => ShowInputDialog("Rename Choice", "Enter new choice name:", currentName);

    private async Task<string?> ShowInputDialog(string title, string message, string defaultValue)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            RequestedThemeVariant = RequestedThemeVariant
        };

        var panel = new StackPanel { Margin = new Thickness(16) };
        var label = new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 8) };
        var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 16) };
        var buttonPanel = new StackPanel 
        { 
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 8
        };
        var okButton = new Button { Content = "OK", Width = 80, IsDefault = true };
        var cancelButton = new Button { Content = "Cancel", Width = 80, IsCancel = true };
        string? result = null;
        okButton.Click += (s, e) => { result = textBox.Text; dialog.Close(); };
        cancelButton.Click += (s, e) => { result = null; dialog.Close(); };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(label);
        panel.Children.Add(textBox);
        panel.Children.Add(buttonPanel);
        dialog.Content = panel;
        dialog.Opened += (s, e) => { textBox.Focus(); textBox.SelectAll(); };
        await dialog.ShowDialog(this);
        return result;
    }
}
