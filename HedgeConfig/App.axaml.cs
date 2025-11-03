using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HedgeConfig.ViewModels;
using HedgeConfig.Views;
using HedgeConfig.Services;

namespace HedgeConfig;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
            
            // Check for updates on startup (runs asynchronously to avoid blocking UI)
            Task.Run(() => UpdateServer.CheckForUpdate());
        }
        base.OnFrameworkInitializationCompleted();
    }
}
