using System.Text;
using System.Text.Json;

namespace StorageSystem.EndToEndTests.Common;

public class ApiClient(HttpClient httpClient)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<(HttpResponseMessage Message, TOutput? Output)> PostAsync<TOutput>(
        string route,
        object payload
    )
        where TOutput : class
    {
        var response = await httpClient.PostAsync(
            route,
            new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json"
            )
        );

        var output = await DeserializeAsync<TOutput>(response);
        return (response, output);
    }

    public async Task<(HttpResponseMessage Message, TOutput? Output)> GetAsync<TOutput>(
        string route
    )
        where TOutput : class
    {
        var response = await httpClient.GetAsync(route);
        var output = await DeserializeAsync<TOutput>(response);
        return (response, output);
    }

    private async Task<TOutput?> DeserializeAsync<TOutput>(HttpResponseMessage response)
        where TOutput : class
    {
        var body = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(body))
            return null;

        return JsonSerializer.Deserialize<TOutput>(body, _jsonOptions);
    }
}
