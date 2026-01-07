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
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToAddProduct { get; }

    public ProductListViewModel(MainViewModel parent)
    {
        _parent = parent;

        // Команда выбора продукта
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

        // Команда ПОДГРУЗКИ (использует текущий SearchText)
        LoadMoreCommand = ReactiveCommand.CreateFromTask(async () => {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Передаем SearchText вторым аргументом, как мы исправили в GoodsService
                var items = await _goodsService.GetProductsAsync(_offset, SearchText);

                if (items != null && items.Count > 0)
                {
                    foreach (var i in items) Products.Add(i);
                    _offset += items.Count;
                    ApplySorting();
                }
            }
            finally { IsBusy = false; }
        });

        // Команда ОБНОВЛЕНИЯ (сброс и загрузка с нуля)
        RefreshCommand = ReactiveCommand.CreateFromTask(async () => {
            _offset = 0;
            Products.Clear();
            await LoadMoreCommand.Execute();
        });

        GoToAddProduct = ReactiveCommand.Create(() => {
            _parent.ShowAddProduct();
        });

        // ЛОГИКА ЖИВОГО ПОИСКА
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500)) // Ждем полсекунды после ввода
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ =>
            {
                await RefreshCommand.Execute();
            });

        // Начальная загрузка при создании ViewModel
        _ = RefreshCommand.Execute();
    }

    private void ApplySorting()
    {
        if (Products == null || Products.Count <= 1) return;

        var sorted = SelectedSortMode switch
        {
            "Сначала дешевые" => Products.OrderBy(p => p.Price).ToList(),
            "Сначала дорогие" => Products.OrderByDescending(p => p.Price).ToList(),
            "По качеству (состав)" => Products.OrderByDescending(p => p.QualityPercentage).ToList(),
            "По рейтингу (звезды)" => Products.OrderByDescending(p => p.Rating).ToList(),
            "По количеству отзывов" => Products.OrderByDescending(p => p.ReviewsCount).ToList(),
            _ => Products.ToList()
        };

        // Обновляем коллекцию только если порядок изменился
        if (!Products.SequenceEqual(sorted))
        {
            Products.Clear();
            foreach (var p in sorted) Products.Add(p);
        }
    }
}