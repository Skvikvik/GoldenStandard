using Xunit;
using GoldenStandard.Models;

namespace GoldenStandard.Tests;

public class ProductCompositionTests
{
    [Theory]
    [InlineData("сахар", 85)]                                     // 100 - 15
    [InlineData("ПАЛЬМОВОЕ МАСЛО", 80)]                           // 100 - 20
    [InlineData("Состав: ГМО; сахар. Е-добавки!", 50)]            // 100 - 25 - 15 - 10
    [InlineData("сахар, сахарный сироп", 85)]                     // 100 - 15 (один раз)
    [InlineData("гмо, консервант, е-122", 60)]                    // 100 - 25 - 5 - 10
    [InlineData("", 100)]                                         // 100
    [InlineData("вода, мука", 100)]                               // 100
    [InlineData("содержит гмо", 75)]                              // 100 - 25
    public void QualityPercentage_ShouldHandleVariousCompositionFormats(string composition, int expectedScore)
    {
        var product = new Product { Composition = composition };
        var actualScore = product.QualityPercentage;

        // Выводим в лог, если не совпало, чтобы легче было дебажить
        Assert.True(expectedScore == actualScore,
            $"Ошибка на составе '{composition}': Ожидали {expectedScore}, но получили {actualScore}");
    }

    [Fact]
    public void QualityPercentage_ShouldNotBeNegative_EvenWithAwfulComposition()
    {
        // Тест на "выживаемость": если в составе вообще всё вредное сразу
        // Arrange
        var product = new Product
        {
            Composition = "сахар, пальмовое масло, ГМО, е-добавки, консерванты, красители, глутамат"
        };

        // Act
        var result = product.QualityPercentage;

        // Assert
        // Результат должен быть 0 или больше, но никак не отрицательным
        Assert.True(result >= 0, "Процент качества ушел в минус!");
    }
}