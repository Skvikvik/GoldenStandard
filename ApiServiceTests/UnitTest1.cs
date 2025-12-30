using Xunit;
using GoldenStandard.Services;

namespace GoldenStandard.Tests;

public class ApiServiceTests
{
    [Fact]
    public void AccessToken_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange (Подготовка)
        // Сбрасываем токен, чтобы тест был "чистым"
        ApiService.AccessToken = null;
        string expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test_token";

        // Act (Действие)
        ApiService.AccessToken = expectedToken;

        // Assert (Проверка)
        Assert.Equal(expectedToken, ApiService.AccessToken);
    }

    [Fact]
    public void BaseUrl_ShouldBeConfigured()
    {
        // Проверяем, что адрес сервера не пустой и не изменился случайно
        // Это важно, так как от него зависят все сетевые запросы

        // Act
        var url = ApiService.BaseUrl;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(url), "BaseUrl не должен быть пустым");
        Assert.Contains("http", url); // Проверяем наличие протокола
    }

    [Fact]
    public void AccessToken_ShouldHandleNull()
    {
        // Arrange
        ApiService.AccessToken = "some_token";

        // Act
        ApiService.AccessToken = null;

        // Assert
        Assert.Null(ApiService.AccessToken);
    }
}