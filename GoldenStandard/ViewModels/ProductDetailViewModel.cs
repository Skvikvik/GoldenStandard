using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

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

    public ProductDetailViewModel(Product product)
    {
        Product = product;

        Reviews = new ObservableCollection<Review>(product.Reviews ?? new List<Review>());
    }

    public void GoBack() => MainViewModel.Instance.ShowList();

    public async Task SendReviewCommand()
    {
        if (string.IsNullOrWhiteSpace(NewReviewText)) return;

        try
        {
            using var client = new HttpClient();
            ApiService.Authenticate(client);

            var url = $"{ApiService.BaseUrl}/api/goods/reviews/{Product.Id}";

            var payload = new
            {
                text = NewReviewText,
                rating = NewRating
            };

            var response = await client.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                var newReview = new Review
                {
                    Text = NewReviewText,
                    Rating = NewRating,
                    User = new User()
                };
                Reviews.Insert(0, newReview);

                NewReviewText = "";
                NewRating = 5;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] Не удалось отправить отзыв: {ex.Message}");
        }
    }
}