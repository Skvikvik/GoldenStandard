using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GoldenStandard.Models;

public class Review
{
    public string User { get; set; } = "Пользователь";
    public string Text { get; set; } = "";
    public int Stars { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}

public class Product
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Description { get; set; } = "Продукт проверен алгоритмом 'Золотой Стандарт'.";
    public double Price { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public ObservableCollection<Review> Reviews { get; set; } = new();

    public double AverageRating => Reviews.Any() ? Math.Round(Reviews.Average(r => r.Stars), 1) : 0.0;

    public int QualityScore
    {
        get
        {
            if (Ingredients == null || !Ingredients.Any()) return 0;
            double score = 100.0;
            var penalties = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "Сахар", 8.0 }, { "Пальмовое масло", 20.0 }, { "Е-", 15.0 },
                { "Консервант", 10.0 }, { "Ароматизатор", 4.0 }, { "Глутамат", 20.0 }
            };
            foreach (var ingredient in Ingredients)
                foreach (var penalty in penalties)
                    if (ingredient.Contains(penalty.Key, StringComparison.OrdinalIgnoreCase)) score -= penalty.Value;
            return (int)Math.Clamp(score, 0, 100);
        }
    }
    public string QualityColor => QualityScore > 80 ? "#27AE60" : (QualityScore > 50 ? "#F39C12" : "#C0392B");
}