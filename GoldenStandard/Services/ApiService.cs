using System.Net.Http;
using System.Net.Http.Headers;

namespace GoldenStandard.Services;

public static class ApiService
{
    public const string BaseUrl = "http://sarafan.bgig.ru";

    public static string? AccessToken { get; set; }

    public static void Authenticate(HttpClient client)
    {
        if (!string.IsNullOrEmpty(AccessToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AccessToken);
        }
    }
}