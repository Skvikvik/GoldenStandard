using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class AddProductViewModel : ReactiveObject
{
    private readonly MainViewModel _parent;
    private readonly GoodsService _goodsService = new();

    private string _name = "";
    private string _description = "";
    private decimal _price;
    private string _imageUrl = "";
    private bool _isBusy;

    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
    public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
    public decimal Price { get => _price; set => this.RaiseAndSetIfChanged(ref _price, value); }
    public string ImageUrl { get => _imageUrl; set => this.RaiseAndSetIfChanged(ref _imageUrl, value); }
    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public AddProductViewModel(MainViewModel parent)
    {
        _parent = parent;

        var canSave = this.WhenAnyValue(
            x => x.Name,
            x => x.Price,
            x => x.Description,
            x => x.IsBusy,
            (n, p, d, b) =>
                !string.IsNullOrWhiteSpace(n) &&
                p > 0 &&
                !string.IsNullOrWhiteSpace(d) &&
                !b);

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsBusy = true;
            try
            {
                var success = await _goodsService.AddProductAsync(Name, Description, Price, ImageUrl);

                if (success)
                {
                    // Обновляем список, чтобы новый товар появился сразу
                    if (_parent.ProductList != null)
                    {
                        await _parent.ProductList.ResetAndReloadAsync();
                    }
                    _parent.ShowMainList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }, canSave);

        CancelCommand = ReactiveCommand.Create(() => _parent.ShowMainList());
    }
}