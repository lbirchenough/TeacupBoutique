using System.Text.Json.Serialization;

namespace orders.Services;

public class TurnstileService(HttpClient httpClient, IConfiguration config)
{
    public async Task<bool> VerifyAsync(string token)
    {
        var secret = config["Turnstile:SecretKey"]!;
        try
        {
            var response = await httpClient.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secret,
                    ["response"] = token
                }));
            var json = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
            return json?.Success ?? false;
        }
        catch
        {
            return false;
        }
    }
}

record TurnstileResponse([property: JsonPropertyName("success")] bool Success);
