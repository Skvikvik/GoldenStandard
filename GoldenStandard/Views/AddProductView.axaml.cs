using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GoldenStandard.Views; // ѕ–ќ¬≈–№: должно быть именно так

public partial class AddProductView : UserControl
{
    public AddProductView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}