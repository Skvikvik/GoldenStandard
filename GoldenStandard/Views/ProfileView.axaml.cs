using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GoldenStandard.Views; // Проверьте, что здесь именно .Views

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    // Если генератор имен все еще сбоит, этот метод поможет загрузить XAML вручную
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}