using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GoldenStandard.ViewModels;

namespace GoldenStandard.Views;

public partial class ProductListView : ReactiveUserControl<ProductListViewModel>
{
    public ProductListView()
    {
        InitializeComponent();
        // Мы убрали логику ScrollChanged, теперь всё работает только через кнопки и команды ViewModel
    }
}