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

    // Адрес вашего сервера (подставьте свой)
    private const string BaseUrl = "http://127.0.0.1:8000";

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
                var items = await _goodsService.GetProductsAsync(_offset, SearchText);

                if (items != null && items.Count > 0)
                {
                    foreach (var i in items)
                    {
                        Products.Add(i);
                        // ЗАПУСК ФОНОВОЙ ЗАГРУЗКИ КАРТИНКИ
                        // Мы не пишем await, чтобы картинки грузились параллельно 
                        // и не блокировали отрисовку списка
                        _ = i.LoadImageAsync(BaseUrl);
                    }
                    _offset += items.Count;
                    ApplySorting();
                }
            }
            finally { IsBusy = false; }
        });

        RefreshCommand = ReactiveCommand.CreateFromTask(async () => {
            _offset = 0;
            Products.Clear();
            await LoadMoreCommand.Execute();
        });

        GoToAddProduct = ReactiveCommand.Create(() => {
            _parent.ShowAddProduct();
        });

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ =>
            {
                await RefreshCommand.Execute();
            });

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

        if (!Products.SequenceEqual(sorted))
        {
            // Используем Move для сохранения объектов, чтобы не перезагружать картинки заново
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = Products.IndexOf(sorted[i]);
                if (oldIndex != i && oldIndex != -1)
                    Products.Move(oldIndex, i);
            }
        }
    }
}