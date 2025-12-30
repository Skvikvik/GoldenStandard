using GoldenStandard.ViewModels;

[Fact]
public async Task Register_PasswordsMismatch_ShowsError()
{
    // Arrange
    var mainVm = new MainViewModel();
    var regVm = new RegistrationViewModel(mainVm)
    {
        Username = "new_user",
        Password = "password123",
        ConfirmPassword = "different_password"
    };

    // Act
    await regVm.RegisterCommand.Execute();

    // Assert
    Assert.Contains("совпадают", regVm.ErrorMessage.ToLower() ?? "");
}