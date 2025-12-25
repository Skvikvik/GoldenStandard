using System;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace GoldenStandard.ViewModels;

public class LoginViewModel : ReactiveObject
{
    private string _email = "";
    private string _password = "";
    private string _errorMessage = "";

    public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToRegister { get; }

    public LoginViewModel(MainViewModel parent)
    {
        var canLogin = this.WhenAnyValue(
            x => x.Email,
            x => x.Password,
            (email, pass) =>
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(pass) &&
                pass.Length >= 8 &&
                pass.Any(char.IsUpper)
        );

        LoginCommand = ReactiveCommand.Create(parent.ShowMainList, canLogin);
        GoToRegister = ReactiveCommand.Create(parent.ShowRegistration);

        this.WhenAnyValue(x => x.Email, x => x.Password)
            .Subscribe(_ => UpdateErrorMessage());
    }

    private void UpdateErrorMessage()
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