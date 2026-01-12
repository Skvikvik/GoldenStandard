using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace GoldenStandard.Models;

public class Product : ReactiveObject
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";

    [JsonPropertyName("composition")]
    public string Composition
    {
        get => _composition;
        set => this.RaiseAndSetIfChanged(ref _composition, value);
    }
    private string _composition = "";

    [JsonPropertyName("description")]
    public string DescriptionSetter { set => Composition = value; }

    [JsonPropertyName("price")] public decimal Price { get; set; }

    private double _baseRating;
    [JsonPropertyName("rating")]
    public double Rating
    {
        get => (Reviews != null && Reviews.Any()) ? Reviews.Average(r => r.Rating) : _baseRating;
        set { _baseRating = value; this.RaisePropertyChanged(); }
    }

    private int _manualReviewsCount;
    [JsonPropertyName("reviews_count")]
    public int ReviewsCount
    {
        get => Math.Max(_manualReviewsCount, Reviews?.Count ?? 0);
        set => this.RaiseAndSetIfChanged(ref _manualReviewsCount, value);
    }

    private ObservableCollection<Review> _reviews = new();
    [JsonPropertyName("reviews")]
    public ObservableCollection<Review> Reviews
    {
        get => _reviews;
        set
        {
            if (_reviews != null) _reviews.CollectionChanged -= (s, e) => RefreshRatingUI();
            _reviews = value;
            this.RaisePropertyChanged();
            if (_reviews != null)
            {
                _reviews.CollectionChanged += (s, e) => RefreshRatingUI();
                RefreshRatingUI();
            }
        }
    }

    [JsonPropertyName("image_url")] public string Image { get; set; } = "";

    private Bitmap? _imageSource;
    [JsonIgnore]
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    [JsonIgnore] public string ReviewsCountBracket => ReviewsCount <= 0 ? "(нет отзывов)" : $"({ReviewsCount})";
    [JsonIgnore] public bool HasReviews => ReviewsCount > 0;

    [JsonIgnore]
    public int QualityPercentage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Composition)) return 0;

            // 1. Список вредных добавок (можно дополнять)
            string[] harmfulIngredients = {
            "сахар", "е621", "пальмовое", "краситель",
            "ароматизатор", "консервант", "глутамат", "кофеин",
            "колер", "кислота"
        };

            // 2. Разделяем состав по запятым и очищаем каждый элемент
            var ingredients = Composition.Split(new[] { ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(i => i.Trim().ToLower())
                                         .ToList();

            if (ingredients.Count == 0) return 0;

            int harmfulCount = 0;

            // 3. Проверяем каждый элемент состава
            foreach (var ingredient in ingredients)
            {
                // Если в элементе (например, "сахарный колер IV") есть хоть одно вредное слово
                if (harmfulIngredients.Any(h => ingredient.Contains(h)))
                {
                    harmfulCount++;
                }
            }

            // 4. Расчет: (Всего - Вредных) / Всего * 100
            double healthScore = (double)(ingredients.Count - harmfulCount) / ingredients.Count;
            int result = (int)(healthScore * 100);

            return Math.Max(0, result); // Чтобы не ушло в минус
        }
    }

    public void RefreshRatingUI()
    {
        this.RaisePropertyChanged(nameof(Rating));
        this.RaisePropertyChanged(nameof(ReviewsCount));
        this.RaisePropertyChanged(nameof(ReviewsCountBracket));
        this.RaisePropertyChanged(nameof(HasReviews));
        this.RaisePropertyChanged(nameof(QualityPercentage));
        this.RaisePropertyChanged(nameof(Composition));
    }

    public async Task LoadImageAsync(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(Image) || ImageSource != null) return;
        try
        {
            using var client = new HttpClient();
            string fullUrl = Image.StartsWith("http") ? Image : $"{baseUrl.TrimEnd('/')}/{Image.TrimStart('/')}";
            var bytes = await client.GetByteArrayAsync(fullUrl);
            using var stream = new MemoryStream(bytes);
            ImageSource = new Bitmap(stream);
        }
        catch { }
    }
}