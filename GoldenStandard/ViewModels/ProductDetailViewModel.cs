using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Reactive;
using System.Linq;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;
using Avalonia.Threading;

namespace GoldenStandard.ViewModels;

public class ProductDetailViewModel : ReactiveObject
{
    private readonly ReviewService _reviewService = new();
    private readonly GoodsService _goodsService = new();
    private User? _currentUser;

    public Product Product { get; }
    public ObservableCollection<Review> Reviews => Product.Reviews;
    public List<int> StarOptions { get; } = new() { 1, 2, 3, 4, 5 };

    private string _newReviewText = "";
    public string NewReviewText { get => _newReviewText; set => this.RaiseAndSetIfChanged(ref _newReviewText, value); }

    private int _newRating = 5;
    public int NewRating { get => _newRating; set => this.RaiseAndSetIfChanged(ref _newRating, value); }

    public ReactiveCommand<Unit, Unit> SendReviewCommand { get; }
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteProductCommand { get; }

    public ProductDetailViewModel(Product product)
    {
        Product = product;
        _ = Product.LoadImageAsync(ApiService.BaseUrl);
        _ = LoadUserProfile();

        SendReviewCommand = ReactiveCommand.CreateFromTask(SendReviewAsync);
        GoBackCommand = ReactiveCommand.Create(() => MainViewModel.Instance.ShowList());

        DeleteProductCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var success = await _goodsService.DeleteProductAsync(Product.Id);
            if (success)
            {
                // Удаляем товар из локальной памяти списка
                if (MainViewModel.Instance.ProductList != null)
                {
                    MainViewModel.Instance.ProductList.RemoveProductFromCache(Product.Id);
                }

                // Возврат к списку
                MainViewModel.Instance.ShowList();
            }
        });
    }

    private async Task LoadUserProfile()
    {
        try { _currentUser = await ApiService.GetProfileAsync(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Ошибка профиля: {ex.Message}"); }
    }

    private async Task SendReviewAsync()
    {
        if (string.IsNullOrWhiteSpace(NewReviewText)) return;
        var (success, error) = await _reviewService.AddReviewAsync(Product.Id, NewReviewText, NewRating);
        if (success)
        {
            if (_currentUser == null) _currentUser = await ApiService.GetProfileAsync();
            var newReview = new Review
            {
                Text = NewReviewText,
                Rating = NewRating,
                User = new User { Username = _currentUser?.Username ?? "guest" }
            };

            Dispatcher.UIThread.Post(() => {
                Product.Reviews.Insert(0, newReview);
                Product.RefreshRatingUI();
                NewReviewText = "";
                NewRating = 5;
            });
        }
    }
}