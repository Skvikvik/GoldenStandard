using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class LoginViewModel : ReactiveObject
{
    private string _username = "";
    private string _password = "";
    private string _errorMessage = "";
    private bool _isBusy;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToRegister { get; }

    public LoginViewModel(MainViewModel parent)
    {
        var canLogin = this.WhenAnyValue(
            x => x.Username, x => x.Password, x => x.IsBusy,
            (u, p, b) => !string.IsNullOrWhiteSpace(u) && !string.IsNullOrWhiteSpace(p) && !b
        );

        LoginCommand = ReactiveCommand.CreateFromTask(async () => await OnLogin(parent), canLogin);
        GoToRegister = ReactiveCommand.Create(parent.ShowRegistration);
    }

    private async Task OnLogin(MainViewModel parent)
    {
        IsBusy = true;
        ErrorMessage = "";

        try
        {
            using var client = new HttpClient();
            var loginData = new { username = Username, password = Password };

            var url = $"{ApiService.BaseUrl}/api/auth/login";
            var response = await client.PostAsJsonAsync(url, loginData);

            if (response.IsSuccessStatusCode)
            {
                var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authData != null && !string.IsNullOrEmpty(authData.access_token))
                {
                    ApiService.AccessToken = authData.access_token;
                    parent.ShowMainList();
                }
                else
                {
                    ErrorMessage = "Сервер не прислал токен доступа.";
                }
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка подключения к серверу";
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
        }
        finally { IsBusy = false; }
    }
}