using System;
using System.Globalization;
using System.Net.Http;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace GoldenStandard.Converters
{
    public class UrlToBitmapConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http")) return null;

                try
                {
                    var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
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