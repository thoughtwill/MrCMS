using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Providers.Text;

public class OllamaAiTextProvider : IAiTextProvider
{
    private readonly OllamaAiSettings _settings;
    private readonly HttpClient _httpClient;

    public OllamaAiTextProvider(OllamaAiSettings settings, HttpClient httpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<AiTextRawResponse> StreamResponseAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Prepare the request body as a JSON object
        var requestBody = new
        {
            model = _settings.Model,
            prompt = prompt,
            stream = true
        };

        // Serialize the request body to JSON
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Prepare the HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
        {
            Content = jsonContent
        };

        // Send the request and stream the response
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Read the response stream line by line
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            // Parse the JSON line
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            // If the response indicates completion, break out of the loop.
            if (root.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
            {
                yield break;
            }

            // Extract the "response" property if available
            if (root.TryGetProperty("response", out var responseElement))
            {
                var chunk = responseElement.GetString() ?? string.Empty;
                yield return new AiTextRawResponse { Chunk = chunk };
            }
        }
    }
}