using System;
using System.Collections.Generic;
using System.Linq;

namespace GoldenStandard.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Composition { get; set; } = "";
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public List<Review> Reviews { get; set; } = new();

    public string Image { get; set; } = "";

    public int QualityPercentage
    {
        get
        {
            int score = 100;
            string comp = (Composition ?? "").ToLower();
            string desc = (Description ?? "").ToLower();
            string fullText = comp + " " + desc;

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