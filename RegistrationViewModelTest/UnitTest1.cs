using GoldenStandard.ViewModels;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace GoldenStandard.Tests;

public class RegistrationViewModelTests
{
    // 1. ТЕСТ ВАЛИДАЦИИ ПАРОЛЯ И ЛОГИНА
    [Theory]
    [InlineData("", "", false)]             // Пусто
    [InlineData("user", "1234567", false)]  // Пароль слишком короткий (7 символов)
    [InlineData("user", "12345678", true)]   // Ровно 8 символов - должно работать
    [InlineData("  ", "123456789", false)]  // Логин только из пробелов
    [InlineData("admin", "securePassword123", true)] // Корректные данные
    public async Task RegisterCommand_ShouldValidateInputCorrectly(string user, string pass, bool expected)
    {
        // Arrange
        var viewModel = new RegistrationViewModel(new MainViewModel());

        // Act
        viewModel.Username = user;
        viewModel.Password = pass;

        // Ждем обновления состояния из потока CanRegister
        var canRegister = await viewModel.RegisterCommand.CanExecute.FirstAsync();

        // Assert
        Assert.Equal(expected, canRegister);
    }

    // 2. ТЕСТ БЛОКИРОВКИ ПРИ ЗАГРУЗКЕ (IsBusy)
    [Fact]
    public async Task RegisterCommand_ShouldBeDisabled_WhenIsBusy()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(new MainViewModel());
        viewModel.Username = "validUser";
        viewModel.Password = "validPassword123";

        // Act
        viewModel.IsBusy = true; // Имитируем активный запрос

        // Assert
        var canRegister = await viewModel.RegisterCommand.CanExecute.FirstAsync();
        Assert.False(canRegister);
    }

    // 3. ТЕСТ СБРОСА ОШИБКИ
    [Fact]
    public async Task OnRegister_ShouldClearPreviousErrorMessage()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(new MainViewModel());
        viewModel.ErrorMessage = "Ошибка с прошлого раза";
        viewModel.Username = "newuser";
        viewModel.Password = "password888";

        // Act
        // Запускаем без ожидания (так как HttpClient упадет в тесте)
        var task = viewModel.RegisterCommand.Execute().ToTask();

        // Assert
        Assert.Equal(string.Empty, viewModel.ErrorMessage);

        try { await task; } catch { /* ожидаем ошибку сети */ }
    }

    // 4. ТЕСТ ПЕРЕХОДА НА ЛОГИН (GoToLogin)
    [Fact]
    public void GoToLogin_Command_ShouldExist()
    {
        // Arrange
        var mainVm = new MainViewModel();
        var regVm = new RegistrationViewModel(mainVm);

        // Act & Assert
        // Проверяем, что команда создана и может быть выполнена
        Assert.NotNull(regVm.GoToLogin);
        Assert.True(regVm.GoToLogin.CanExecute.FirstAsync().GetAwaiter().GetResult());
    }
}