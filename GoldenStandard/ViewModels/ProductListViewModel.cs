using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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
    private string _searchText = "";
    private string _selectedSortMode = "Сначала дешевые";

    public ObservableCollection<Product> Products { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string[] SortModes { get; } =
    {
        "Сначала дешевые",
        "Сначала дорогие",
        "По качеству (состав)",
        "По рейтингу (звезды)",
        "По количеству отзывов"
    };

    public string SelectedSortMode
    {
        get => _selectedSortMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSortMode, value);
            ApplySorting();
        }
    }

    public ReactiveCommand<Product, Unit> SelectProductCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToAddProduct { get; }

    public ProductListViewModel(MainViewModel parent)
    {
        _parent = parent;

        SelectProductCommand = ReactiveCommand.CreateFromTask<Product>(async (p) => {
            if (p == null || IsBusy) return;
            IsBusy = true;
            try
            {
                var full = await _goodsService.GetProductDetailsAsync(p.Id);
                _parent.ShowProductDetail(full ?? p);
            }
            finally { IsBusy = false; }
        });

        LoadMoreCommand = ReactiveCommand.CreateFromTask(async () => {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var items = await _goodsService.GetProductsAsync(_offset);
                if (items != null)
                {
                    foreach (var i in items) Products.Add(i);
                    _offset += items.Count;
                    ApplySorting();
                }
            }
            finally { IsBusy = false; }
        });

        GoToAddProduct = ReactiveCommand.Create(() => {
            _parent.ShowAddProduct();
        });

        _ = LoadMoreCommand.Execute();
    }

    private void ApplySorting()
    {
        if (Products == null || Products.Count <= 1) return;

        IEnumerable<Product> sortedQuery = SelectedSortMode switch
        {
            "Сначала дешевые" => Products.OrderBy(p => p.Price),
            "Сначала дорогие" => Products.OrderByDescending(p => p.Price),
            "По качеству (состав)" => Products.OrderByDescending(p => p.QualityPercentage),
            "По рейтингу (звезды)" => Products.OrderByDescending(p => p.Rating),
            "По количеству отзывов" => Products.OrderByDescending(p => p.ReviewsCount),
            _ => Products.ToList()
        };

        var sortedList = sortedQuery.ToList();
        Products.Clear();
        foreach (var p in sortedList) Products.Add(p);
    }
}