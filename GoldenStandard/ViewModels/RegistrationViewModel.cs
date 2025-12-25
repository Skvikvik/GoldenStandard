using System;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace GoldenStandard.ViewModels;

public class RegistrationViewModel : ReactiveObject
{
    private string _email = "";
    private string _password = "";
    private string _errorMessage = "";

    public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }

    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToLogin { get; }

    public RegistrationViewModel(MainViewModel parent)
    {
        // Кнопка активна только если данные верны
        var canRegister = this.WhenAnyValue(
            x => x.Email,
            x => x.Password,
            (email, pass) =>
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(pass) &&
                pass.Length >= 8 &&
                pass.Any(char.IsUpper)
        );

        RegisterCommand = ReactiveCommand.Create(parent.ShowMainList, canRegister);
        GoToLogin = ReactiveCommand.Create(parent.ShowLogin);

        // Следим за вводом для показа ошибок
        this.WhenAnyValue(x => x.Email, x => x.Password)
            .Subscribe(_ => UpdateError());
    }

    private void UpdateError()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            ErrorMessage = "Заполните все поля";
        else if (Password.Length < 8)
            ErrorMessage = "Пароль должен содержать не менее 8 символов";
        else if (!Password.Any(char.IsUpper))
            ErrorMessage = "Пароль должен содержать заглавную букву";
        else
            ErrorMessage = "";
    }
}