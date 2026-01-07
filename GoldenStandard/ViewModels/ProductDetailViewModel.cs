using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;
using Avalonia.Threading;

namespace GoldenStandard.ViewModels;

public class ProductDetailViewModel : ReactiveObject
{
    public Product Product { get; }
    public ObservableCollection<Review> Reviews { get; }
    public List<int> StarOptions { get; } = new() { 1, 2, 3, 4, 5 };

    private string _newReviewText = "";
    public string NewReviewText
    {
        get => _newReviewText;
        set => this.RaiseAndSetIfChanged(ref _newReviewText, value);
    }

    private int _newRating = 5;
    public int NewRating
    {
        get => _newRating;
        set => this.RaiseAndSetIfChanged(ref _newRating, value);
    }

    // Команды для привязки к кнопкам в AXAML
    public ReactiveCommand<Unit, Unit> SendReviewCommand { get; }
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public ProductDetailViewModel(Product product)
    {
        Product = product;
        Reviews = new ObservableCollection<Review>(product.Reviews ?? new List<Review>());

        // Инициализация команд
        SendReviewCommand = ReactiveCommand.CreateFromTask(SendReviewAsync);
        GoBackCommand = ReactiveCommand.Create(() => MainViewModel.Instance.ShowList());
    }

    private async Task SendReviewAsync()
    {
        // Базовая валидация перед отправкой
        if (string.IsNullOrWhiteSpace(NewReviewText)) return;

        try
        {
            using var client = new HttpClient();
            ApiService.Authenticate(client);

            var url = $"{ApiService.BaseUrl}/api/goods/reviews/{Product.Id}";
            var payload = new { text = NewReviewText, rating = NewRating };

            var response = await client.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                // Создаем объект отзыва
                var newReview = new Review
                {
                    Text = NewReviewText,
                    Rating = NewRating,
                    User = new User { Username = "Вы" }
                };

                // Обновляем UI в основном потоке
                Dispatcher.UIThread.Post(() =>
                {
                    Reviews.Insert(0, newReview);
                    NewReviewText = "";
                    NewRating = 5;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending review: {ex.Message}");
        }
    }
}