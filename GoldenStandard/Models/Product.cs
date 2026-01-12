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

    private string _composition = "";
    [JsonPropertyName("composition")]
    public string Composition
    {
        get => _composition;
        set => this.RaiseAndSetIfChanged(ref _composition, value);
    }

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

    [JsonPropertyName("image_url")]
    public string Image { get; set; } = "";

    // Псевдоним для совместимости с ViewModel
    [JsonIgnore]
    public string ImageUrl
    {
        get => Image;
        set => Image = value;
    }

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
            string[] harmfulIngredients = { "сахар", "е621", "пальмовое", "краситель", "ароматизатор", "консервант", "глутамат", "кофеин", "колер", "кислота" };
            var ingredients = Composition.Split(new[] { ',', '.' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(i => i.Trim().ToLower()).ToList();
            if (ingredients.Count == 0) return 0;
            int harmfulCount = ingredients.Count(ingredient => harmfulIngredients.Any(h => ingredient.Contains(h)));
            return Math.Max(0, (int)((double)(ingredients.Count - harmfulCount) / ingredients.Count * 100));
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
        if (string.IsNullOrWhiteSpace(Image)) return;
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