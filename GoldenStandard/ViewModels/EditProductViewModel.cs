using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Reactive;
using System.Linq;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class EditProductViewModel : ReactiveObject
{
    private readonly Product _originalProduct;

    private string _name;
    private string _description;
    private decimal _price;
    private string _composition;

    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
    public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
    public decimal Price { get => _price; set => this.RaiseAndSetIfChanged(ref _price, value); }
    public string Composition { get => _composition; set => this.RaiseAndSetIfChanged(ref _composition, value); }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public EditProductViewModel(Product p)
    {
        _originalProduct = p;

        // Загружаем текущие данные в поля ввода
        Name = p.Name;
        Description = p.Description;
        Price = p.Price;
        Composition = p.Composition;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveToDatabaseAsync);
        CancelCommand = ReactiveCommand.Create(() => MainViewModel.Instance.ShowProductDetail(p));
    }

    private async Task SaveToDatabaseAsync()
    {
        try
        {
            using var client = new HttpClient();
            // ВАЖНО: Без авторизации сервер отклонит PUT запрос
            ApiService.Authenticate(client);

            var url = $"{ApiService.BaseUrl}/api/goods/products/{_originalProduct.Id}";

            // Формируем объект для JSON. Названия полей должны совпадать с бэкендом
            var payload = new
            {
                name = Name,
                description = Description,
                price = (double)Price,
                composition = Composition
            };

            // Отправляем изменения в базу данных
            var response = await client.PutAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                // Если сервер принял данные, обновляем локальный объект
                _originalProduct.Name = Name;
                _originalProduct.Price = Price;
                _originalProduct.Description = Description;
                _originalProduct.Composition = Composition;

                // Возвращаемся в детали товара, где уже будут новые данные
                MainViewModel.Instance.ShowProductDetail(_originalProduct);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API Error] {response.StatusCode}: {errorBody}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Critical Error] {ex.Message}");
        }
    }
}