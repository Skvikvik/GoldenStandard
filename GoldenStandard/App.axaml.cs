using Avalonia;
using Avalonia.Markup.Xaml;
using GoldenStandard.ViewModels;
using GoldenStandard.Views;

namespace GoldenStandard;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Используем 'object' и 'dynamic', чтобы компилятор не ругался на отсутствие интерфейса
        object lifetime = ApplicationLifetime;

        if (lifetime != null && lifetime.GetType().Name.Contains("ClassicDesktop"))
        {
            // Режим ПК (Windows/Linux/macOS)
            dynamic desktop = lifetime;
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (lifetime != null && lifetime.GetType().Name.Contains("SingleView"))
        {
            // Режим Мобилки/Браузера
            dynamic singleView = lifetime;
            singleView.MainView = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}