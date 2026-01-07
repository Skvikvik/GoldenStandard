using System;
using System.Globalization;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using GoldenStandard.Services; // Чтобы взять ApiService.BaseUrl

namespace GoldenStandard.Converters
{
    public class UrlToBitmapConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                // Если путь относительный (напр. /media/...), добавляем BaseUrl
                if (!url.StartsWith("http"))
                {
                    url = ApiService.BaseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
                }

                try
                {
                    // Внимание: GetResult() — плохо для производительности, 
                    // но для курсового проекта допустимо.
                    var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                        return new Bitmap(stream);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Image Error]: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}