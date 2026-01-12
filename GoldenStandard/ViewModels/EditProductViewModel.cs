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
    private readonly Product _originalProduct;

    private string _name;
    private string _composition;
    private decimal _price;
    private string _imageUrl;
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
        _originalProduct = product;
        _name = product.Name ?? "";
        _composition = product.Composition ?? "";
        _price = product.Price;
        _imageUrl = product.ImageUrl ?? "";

        var canSave = this.WhenAnyValue(
            x => x.Name, x => x.Price, x => x.IsBusy,
            (n, p, b) => !string.IsNullOrWhiteSpace(n) && p > 0 && !b);

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsBusy = true;
            try
            {
                var success = await _goodsService.UpdateProductAsync(_originalProduct.Id, Name, Composition, Price, ImageUrl);
                if (success)
                {
                    _originalProduct.Name = Name;
                    _originalProduct.Composition = Composition;
                    _originalProduct.Price = Price;
                    _originalProduct.ImageUrl = ImageUrl;
                    _ = _originalProduct.LoadImageAsync(ApiService.BaseUrl);

                    _parent.ShowProductDetail(_originalProduct);
                }
            }
            finally { IsBusy = false; }
        }, canSave);

        CancelCommand = ReactiveCommand.Create(() => _parent.ShowProductDetail(_originalProduct));
    }
}