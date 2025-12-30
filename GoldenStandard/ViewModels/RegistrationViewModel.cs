using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using ReactiveUI;
using GoldenStandard.Models;
using GoldenStandard.Services;

namespace GoldenStandard.ViewModels;

public class RegistrationViewModel : ReactiveObject
{
    private string _username = "";
    private string _password = "";
    private string _errorMessage = "";
    private bool _isBusy;

    public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }

    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToLogin { get; }

    public RegistrationViewModel(MainViewModel parent)
    {
        var canRegister = this.WhenAnyValue(
            x => x.Username, x => x.Password, x => x.IsBusy,
            (u, p, busy) => !string.IsNullOrWhiteSpace(u) && !string.IsNullOrWhiteSpace(p) && p.Length >= 8 && !busy
        );

        RegisterCommand = ReactiveCommand.CreateFromTask(async () => await OnRegister(parent), canRegister);
        GoToLogin = ReactiveCommand.Create(parent.ShowLogin);
    }

    private async Task OnRegister(MainViewModel parent)
    {
        IsBusy = true;
        ErrorMessage = "";

        try
        {
            using var client = new HttpClient();

            var payload = new
            {
                username = Username,
                password = Password
            };

            var url = $"{ApiService.BaseUrl}/api/auth/register";
            var response = await client.PostAsJsonAsync(url, payload);
            var responseBody = await response.Content.ReadAsStringAsync();

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
                    parent.ShowLogin();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Код: {response.StatusCode}, Тело: {responseBody}");
                ErrorMessage = "Пользователь уже существует или ошибка данных";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Нет связи с сервером";
            System.Diagnostics.Debug.WriteLine($"Reg Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}