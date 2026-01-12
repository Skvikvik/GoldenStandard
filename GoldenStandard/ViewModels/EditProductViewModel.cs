using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class EditProductViewModel : ReactiveObject
{
    private readonly MainViewModel _parent;
    private readonly GoodsService _goodsService = new();
    private readonly Product _targetProduct;

    private string _name = "";
    private string _composition = ""; // Теперь используем только состав
    private decimal _price;
    private string _imageUrl = "";
    private bool _isBusy;

    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
    public string Composition { get => _composition; set => this.RaiseAndSetIfChanged(ref _composition, value); }
    public decimal Price { get => _price; set => this.RaiseAndSetIfChanged(ref _price, value); }
    public string ImageUrl { get => _imageUrl; set => this.RaiseAndSetIfChanged(ref _imageUrl, value); }
    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public EditProductViewModel(MainViewModel parent, Product product)
    {
        _parent = parent;
        _targetProduct = product;

        // Заполняем поля из существующего продукта
        Name = product.Name;
        Composition = product.Composition; // ИСПРАВЛЕНО: было Description
        Price = product.Price;
        ImageUrl = product.Image;

        var canSave = this.WhenAnyValue(
            x => x.Name, x => x.Price, x => x.Composition, x => x.IsBusy,
            (n, p, c, b) => !string.IsNullOrWhiteSpace(n) && p > 0 && !string.IsNullOrWhiteSpace(c) && !b);

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsBusy = true;
            try
            {
                // Используем тот же метод добавления/обновления
                var success = await _goodsService.AddProductAsync(Name, Composition, Price, ImageUrl);
                if (success)
                {
                    // Обновляем данные в живом объекте списка
                    _targetProduct.Name = Name;
                    _targetProduct.Composition = Composition;
                    _targetProduct.Price = Price;
                    _targetProduct.RefreshRatingUI();

                    _parent.ShowMainList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            finally { IsBusy = false; }
        }, canSave);

        CancelCommand = ReactiveCommand.Create(() => _parent.ShowMainList());
    }
}