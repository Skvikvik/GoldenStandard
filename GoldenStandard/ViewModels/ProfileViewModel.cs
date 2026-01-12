using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using GoldenStandard.Services;
using GoldenStandard.Models;

namespace GoldenStandard.ViewModels
{
    public class UserProfileDto
    {
        public string username { get; set; } = "";
        public string? display_name { get; set; }
        public string? email { get; set; }
        public string? description { get; set; }
    }

    public class ProfileViewModel : ReactiveObject
    {
        private readonly MainViewModel _parent;

        private string _username = "";
        public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }

        private string _displayName = "";
        public string DisplayName { get => _displayName; set => this.RaiseAndSetIfChanged(ref _displayName, value); }

        private string _email = "";
        public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }

        private string _description = "";
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }

        private string _statusMessage = "";
        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

        private string _errorMessage = "";
        public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public ProfileViewModel(MainViewModel parent)
        {
            _parent = parent;

            SaveCommand = ReactiveCommand.CreateFromTask(OnSave);
            LogoutCommand = ReactiveCommand.Create(OnLogout);

            _ = LoadProfile();
        }

        private async Task LoadProfile()
        {
            try
            {
                using var client = new HttpClient();
                ApiService.Authenticate(client);

                var url = $"{ApiService.BaseUrl}/api/profiles/profile/";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (data != null)
                    {
                        Username = data.username;
                        DisplayName = data.display_name ?? "";
                        Email = data.email ?? "";
                        Description = data.description ?? "";
                    }
                }
                else
                {
                    StatusMessage = "Не удалось загрузить данные профиля";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Profile Error: {ex.Message}");
                StatusMessage = "Ошибка связи с сервером";
            }
        }

        private async Task OnSave()
        {
            try
            {
                StatusMessage = "Сохранение...";

                using var client = new HttpClient();
                ApiService.Authenticate(client);

                var url = $"{ApiService.BaseUrl}/api/profiles/profile/";

                var updateData = new UserProfileDto
                {
                    username = this.Username,
                    display_name = this.DisplayName,
                    email = this.Email,
                    description = this.Description
                };

                var response = await client.PutAsJsonAsync(url, updateData);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Данные обновлены!";
                }
                else
                {
                    StatusMessage = "Ошибка сохранения";
                }
            }
            catch
            {
                StatusMessage = "Ошибка соединения";
            }

            await Task.Delay(2000);
            StatusMessage = "";
        }

        private void OnLogout()
        {
            ApiService.AccessToken = null;
            _parent.ShowLogin();
        }
    }
}