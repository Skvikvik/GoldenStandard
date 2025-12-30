using GoldenStandard.Models;
using ReactiveUI;

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

    public MainViewModel()
    {
        _instance = this;
        ShowLogin();
    }


    public void ShowLogin() => CurrentPage = new LoginViewModel(this);

    public void ShowRegistration() => CurrentPage = new RegistrationViewModel(this);

    public void ShowList() => CurrentPage = new ProductListViewModel(this);

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