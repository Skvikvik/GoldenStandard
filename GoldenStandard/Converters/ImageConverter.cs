using System;
using System.Globalization;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using GoldenStandard.Services;
using System.IO;

namespace GoldenStandard.Converters
{
    public class UrlToBitmapConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http") && !url.StartsWith("avares://"))
                {
                    url = ApiService.BaseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
                }

                try
                {

                    var bytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    using var ms = new MemoryStream(bytes);
                    return new Bitmap(ms);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Image Error]: {url} -> {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}