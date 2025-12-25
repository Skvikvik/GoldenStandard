using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using GoldenStandard.Models;
using ReactiveUI;

namespace GoldenStandard.ViewModels;

// ГЛАВНАЯ МОДЕЛЬ (Управляет переключением страниц)
public class MainViewModel : ReactiveObject
{
    private object _currentPage;
    public object CurrentPage { get => _currentPage; set => this.RaiseAndSetIfChanged(ref _currentPage, value); }
    private readonly ProductListViewModel _listVm;

    public MainViewModel()
    {
        _listVm = new ProductListViewModel(this);
        ShowLogin();
    }
    public void ShowLogin() => CurrentPage = new LoginViewModel(this);
    public void ShowRegistration() => CurrentPage = new RegistrationViewModel(this);
    public void ShowMainList() => CurrentPage = _listVm;
    public void ShowProductDetail(Product product) => CurrentPage = new ProductDetailViewModel(this, product);
}

// СПИСОК ТОВАРОВ
public class ProductListViewModel : ReactiveObject
{
    private readonly List<Product> _all;
    private ObservableCollection<Product> _prods;
    public ObservableCollection<Product> Products { get => _prods; set => this.RaiseAndSetIfChanged(ref _prods, value); }

    private string _searchText;
    public string SearchText { get => _searchText; set { this.RaiseAndSetIfChanged(ref _searchText, value); Apply(); } }

    public List<string> Categories { get; } = new() { "Все", "Напитки", "Еда", "Сладости" };
    private string _selectedCategory = "Все";
    public string SelectedCategory { get => _selectedCategory; set { this.RaiseAndSetIfChanged(ref _selectedCategory, value); Apply(); } }

    public List<string> SortModes { get; } = new() { "По умолчанию", "ТОП Качества", "Сначала вредные" };
    private string _mode = "По умолчанию";
    public string SelectedSortMode { get => _mode; set { this.RaiseAndSetIfChanged(ref _mode, value); Apply(); } }

    public ReactiveCommand<Product, Unit> OpenProduct { get; }

    public ProductListViewModel(MainViewModel p)
    {
        _all = new List<Product> {
            new Product { Name = "Молоко 3.2%", Price = 89, Manufacturer = "ЭкоНива", Ingredients = new() {"Молоко"} },
            new Product { Name = "Кока-Кола 0.5л", Price = 99, Manufacturer = "Мултон", Ingredients = new() {"Вода", "Сахар", "Краситель Е-150", "Ортофосфорная кислота"} },
            new Product { Name = "Шоколад Молочный", Price = 110, Manufacturer = "Alpen Gold", Ingredients = new() {"Сахар", "Какао-масло", "Пальмовое масло", "Эмульгатор"} },
            new Product { Name = "Чипсы Сырные", Price = 145, Manufacturer = "Lays", Ingredients = new() {"Картофель", "Пальмовое масло", "Глутамат", "Соль"} },
            new Product { Name = "Яблоко Семеренко", Price = 35, Manufacturer = "Сады Дона", Ingredients = new() {"Яблоко"} }
        };

        var random = new Random();
        foreach (var pr in _all)
            pr.Reviews.Add(new Review { Stars = random.Next(3, 6), Text = "Тестовый отзыв.", User = "Тестер" });

        _prods = new ObservableCollection<Product>(_all);
        OpenProduct = ReactiveCommand.Create<Product>(p.ShowProductDetail);
    }

    private void Apply()
    {
        IEnumerable<Product> filtered = _all;
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedCategory != "Все")
        {
            if (SelectedCategory == "Напитки") filtered = filtered.Where(x => x.Name.Contains("Кола") || x.Name.Contains("Молоко"));
            if (SelectedCategory == "Сладости") filtered = filtered.Where(x => x.Name.Contains("Шоколад"));
        }

        filtered = _mode switch
        {
            "ТОП Качества" => filtered.OrderByDescending(x => x.QualityScore),
            "Сначала вредные" => filtered.OrderBy(x => x.QualityScore),
            _ => filtered
        };

        Products = new ObservableCollection<Product>(filtered);
    }
}

// ДЕТАЛИ ТОВАРА
public class ProductDetailViewModel : ReactiveObject
{
    public Product SelectedProduct { get; }
    public ReactiveCommand<Unit, Unit> GoBack { get; }
    public List<int> StarOptions { get; } = new() { 1, 2, 3, 4, 5 };

    private string _newText;
    public string NewText { get => _newText; set => this.RaiseAndSetIfChanged(ref _newText, value); }

    private int _newStars = 5;
    public int NewStars { get => _newStars; set => this.RaiseAndSetIfChanged(ref _newStars, value); }

    public ReactiveCommand<Unit, Unit> Send { get; }

    public ProductDetailViewModel(MainViewModel p, Product pr)
    {
        SelectedProduct = pr;
        GoBack = ReactiveCommand.Create(p.ShowMainList);
        Send = ReactiveCommand.Create(() => {
            if (!string.IsNullOrEmpty(NewText))
            {
                SelectedProduct.Reviews.Insert(0, new Review { Text = NewText, Stars = NewStars, User = "Вы" });
                NewText = "";
                this.RaisePropertyChanged(nameof(SelectedProduct));
            }
        });
    }
}