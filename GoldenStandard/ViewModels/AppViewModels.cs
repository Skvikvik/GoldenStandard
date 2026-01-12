using System.Threading.Tasks;
using GoldenStandard.Models;
using ReactiveUI;
using Avalonia.Threading;

namespace GoldenStandard.ViewModels;

public class MainViewModel : ReactiveObject
{
    private static MainViewModel _instance;
    public static MainViewModel Instance => _instance;

    private object _currentPage;
    public object CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    private ProductListViewModel _productList;
    public ProductListViewModel ProductList => _productList ??= new ProductListViewModel(this);

    public MainViewModel()
    {
        _instance = this;
        ShowLogin();
    }

    public void ShowLogin() => CurrentPage = new LoginViewModel(this);
    public void ShowRegistration() => CurrentPage = new RegistrationViewModel(this);

    public void ShowList()
    {
        CurrentPage = ProductList;
        Task.Run(async () =>
        {
            await ProductList.LoadProductsAsync();
        });
    }

    public void ShowMainList() => ShowList();
    public void ShowAddProduct() => CurrentPage = new AddProductViewModel(this);

    public void ShowProductDetail(Product p)
    {
        if (p != null)
        {
            CurrentPage = new ProductDetailViewModel(p);
        }
    }

    public void ShowProfile() => CurrentPage = new ProfileViewModel(this);
}