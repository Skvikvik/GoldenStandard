using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GoldenStandard.Views; // Проверьте, что это совпадает с проектом

public partial class RegistrationView : UserControl
{
    public RegistrationView()
    {
        InitializeComponent();
    }

    // Если ошибка не исчезает, добавьте этот метод вручную (старый стиль Avalonia)
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}