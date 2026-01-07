using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace GoldenStandard.Models;

public class Product : ReactiveObject // Добавили базу для уведомлений
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("composition")]
    public string Composition { get; set; } = "";

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("reviews_count")]
    public int ReviewsCount { get; set; }

    [JsonPropertyName("reviews")]
    public List<Review> Reviews { get; set; } = new();

    [JsonPropertyName("image_url")]
    public string Image { get; set; } = "";

    // --- НОВОЕ ДЛЯ КАРТИНОК ---
    private Bitmap? _imageSource;

    [JsonIgnore]
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    // Фоновая загрузка
    public async Task LoadImageAsync(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(Image)) return;
        if (ImageSource != null) return; // Уже загружено

        try
        {
            using var client = new HttpClient();
            string fullUrl = Image.StartsWith("http") ? Image : $"{baseUrl.TrimEnd('/')}/{Image.TrimStart('/')}";

            var bytes = await client.GetByteArrayAsync(fullUrl);
            using var stream = new MemoryStream(bytes);
            ImageSource = new Bitmap(stream);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фото: {ex.Message}");
        }
    }
    // --------------------------

    [JsonIgnore]
    public int QualityPercentage
    {
        get
        {
            int score = 100;
            string fullText = ((Composition ?? "") + " " + (Description ?? "")).ToLower();

            if (fullText.Contains("сахар")) score -= 15;
            if (fullText.Contains("пальмовое масло")) score -= 25;
            if (fullText.Contains("гмо")) score -= 25;
            if (fullText.Contains("е-") || fullText.Contains("добавка е")) score -= 10;
            if (fullText.Contains("консервант")) score -= 5;
            if (fullText.Contains("краситель")) score -= 5;
            if (fullText.Contains("глутамат")) score -= 10;

            return Math.Max(0, score);
        }
    }
}