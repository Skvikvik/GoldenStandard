using Xunit;
using GoldenStandard.ViewModels;
using System.Threading.Tasks;

namespace GoldenStandard.Tests;

public class LoginTests
{
    [Fact]
    public async Task OfflineLogin_Success_NavigatesToProductList()
    {
        // Arrange (Подготовка)
        var mainVm = new MainViewModel(); // Инициализирует AppViewModels.cs
        var loginVm = new LoginViewModel(mainVm)
        {
            Username = "admin",
            Password = "Admin123"
        };

        // Act (Действие)
        // Вызываем команду входа напрямую
        await loginVm.LoginCommand.Execute();

        // Assert (Проверка)
        // 1. Проверяем, что текущая страница сменилась на список товаров
        Assert.IsType<ProductListViewModel>(mainVm.CurrentPage);
        // 2. Проверяем, что установился оффлайн токен
        // (Доступ к ApiService.AccessToken)
    }

    [Fact]
    public async Task Login_WithEmptyFields_CanLoginIsFalse()
    {
        // Arrange
        var mainVm = new MainViewModel();
        var loginVm = new LoginViewModel(mainVm);

        // Act & Assert
        // Проверяем работу canLogin (ReactiveCommand свойство)
        // Если поля пустые, команда должна быть заблокирована
        var canExecute = await loginVm.LoginCommand.CanExecute.FirstAsync();
        Assert.False(canExecute);
    }

    [Fact]
    public void GoToRegister_SwitchesPageToRegistration()
    {
        // Arrange
        var mainVm = new MainViewModel();
        var loginVm = new LoginViewModel(mainVm);

        // Act
        loginVm.GoToRegister.Execute().Subscribe();

        // Assert
        Assert.IsType<RegistrationViewModel>(mainVm.CurrentPage);
    }

    [Fact]
    public async Task WrongCredentials_ShowsErrorMessage()
    {
        // Arrange
        var mainVm = new MainViewModel();
        var loginVm = new LoginViewModel(mainVm)
        {
            Username = "wrongUser",
            Password = "wrongPassword"
        };

        // Act
        await loginVm.LoginCommand.Execute();

        // Assert
        // Поскольку сервера нет, сработает блок catch или проверка на admin
        Assert.NotEmpty(loginVm.ErrorMessage);
        Assert.IsType<LoginViewModel>(mainVm.CurrentPage); // Страница не должна измениться
    }
}