using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace GoldenStandard;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(); // ПРОВЕРЬ ЭТУ СТРОКУ
}