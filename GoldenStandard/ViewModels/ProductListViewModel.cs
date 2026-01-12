using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class ProductListViewModel : ReactiveObject
{
    private readonly MainViewModel _parent;
    private readonly GoodsService _goodsService = new();
    private int _offset = 0;
    private bool _isBusy;
    private bool _canLoadMore = true;
    private string _searchText = "";
    private string _selectedSortMode = "Сначала дешевые";

    private readonly List<Product> _allLoadedProducts = new();
    public ObservableCollection<Product> Products { get; } = new();

    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }
    public string SearchText { get => _searchText; set => this.RaiseAndSetIfChanged(ref _searchText, value); }
    public bool CanLoadMore { get => _canLoadMore; set => this.RaiseAndSetIfChanged(ref _canLoadMore, value); }
    public string[] SortModes { get; } = { "Сначала дешевые", "Сначала дорогие", "По качеству (состав)", "По рейтингу (звезды)", "По количеству отзывов" };

    public string SelectedSortMode
    {
        get => _selectedSortMode;
        set { this.RaiseAndSetIfChanged(ref _selectedSortMode, value); ApplyFilterAndSort(); }
    }

    public ReactiveCommand<Product, Unit> SelectProductCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToAddProduct { get; }

    public ProductListViewModel(MainViewModel parent)
    {
        _parent = parent;

        LoadMoreCommand = ReactiveCommand.CreateFromTask(async () => {
            if (IsBusy || !CanLoadMore) return;
            IsBusy = true;
            try
            {
                int limit = 50;
                var items = await _goodsService.GetProductsAsync(_offset, limit);
                if (items != null && items.Count > 0)
                {
                    foreach (var i in items)
                    {
                        if (_allLoadedProducts.All(p => p.Id != i.Id))
                        {
                            _allLoadedProducts.Add(i);
                            _ = i.LoadImageAsync(ApiService.BaseUrl);
                        }
                    }
                    _offset += items.Count;
                    CanLoadMore = (items.Count == limit);
                    ApplyFilterAndSort();
                }
                else { CanLoadMore = false; }
            }
            finally { IsBusy = false; }
        });

        RefreshCommand = ReactiveCommand.CreateFromTask(async () => {
            if (IsBusy) return;
            _offset = 0;
            CanLoadMore = true;
            _allLoadedProducts.Clear();
            Products.Clear();
            await LoadMoreCommand.Execute();
        });

        SelectProductCommand = ReactiveCommand.CreateFromTask<Product>(async (p) => {
            if (p == null || IsBusy) return;
            IsBusy = true;
            try
            {
                var full = await _goodsService.GetProductDetailsAsync(p.Id);
                if (full != null)
                {
                    p.Composition = full.Composition;
                    p.Reviews = full.Reviews;
                    p.Rating = full.Rating;
                    p.ReviewsCount = full.ReviewsCount;
                    p.RefreshRatingUI();
                }
                _parent.ShowProductDetail(p);
            }
            finally { IsBusy = false; }
        });

        GoToAddProduct = ReactiveCommand.Create(() => _parent.ShowAddProduct());

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFilterAndSort());

        _ = RefreshCommand.Execute();
    }

    // Принудительная перезагрузка для новых товаров
    public async Task ResetAndReloadAsync() => await RefreshCommand.Execute();

    // Быстрое удаление из памяти для удаленных товаров
    public void RemoveProductFromCache(int productId)
    {
        var item = _allLoadedProducts.FirstOrDefault(p => p.Id == productId);
        if (item != null)
        {
            _allLoadedProducts.Remove(item);
            ApplyFilterAndSort();
        }
    }

    public async Task LoadProductsAsync()
    {
        if (_allLoadedProducts.Count == 0 && !IsBusy) await RefreshCommand.Execute();
    }

    private void ApplyFilterAndSort()
    {
        string query = (SearchText ?? "").Trim().ToLower();
        var filtered = _allLoadedProducts
            .Where(p => string.IsNullOrWhiteSpace(query) || (p.Name != null && p.Name.ToLower().Contains(query)))
            .ToList();

        IEnumerable<Product> sorted;
        switch (SelectedSortMode)
        {
            case "Сначала дорогие": sorted = filtered.OrderByDescending(p => p.Price); break;
            case "По качеству (состав)": sorted = filtered.OrderByDescending(p => p.QualityPercentage); break;
            case "По рейтингу (звезды)": sorted = filtered.OrderByDescending(p => p.Rating); break;
            case "По количеству отзывов": sorted = filtered.OrderByDescending(p => p.ReviewsCount); break;
            default: sorted = filtered.OrderBy(p => p.Price); break;
        }

        var finalResult = sorted.ToList();
        Products.Clear();
        foreach (var p in finalResult) Products.Add(p);
    }
}