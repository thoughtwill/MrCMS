using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Providers;

public class OpenAiProviderService : IAiProvider
{
    private readonly OpenAiSettings _settings;
    private readonly HttpClient _httpClient;

    public OpenAiProviderService(OpenAiSettings settings, HttpClient httpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<AiRawResponse> StreamResponseAsync(string prompt)
    {
        // Prepare the request body as a JSON object.
        // Adjust properties as necessary depending on your OpenAI endpoint.
        var requestBody = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content =
                        "You are a helpful assistant integrated into a CMS. Always respond in the requested format."
                },
                new { role = "user", content = prompt }
            },
            stream = true
        };

        // Serialize the request body to JSON.
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Prepare the HTTP request.
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
        {
            Content = jsonContent
        };

        // Add the Authorization header if an API key is provided.
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        // Send the request and configure the client to read the headers first.
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Open the response stream.
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        // Read the response stream line by line.
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            // Skip any empty lines.
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // OpenAI streams use Server-Sent Events, where each line starts with "data: ".
            if (line.StartsWith("data: "))
            {
                // Remove the "data: " prefix.
                var data = line.Substring("data: ".Length).Trim();

                // Check for the stream termination signal.
                if (data == "[DONE]")
                {
                    yield break;
                }

                JsonElement root;
                try
                {
                    // Parse the JSON payload.
                    using var document = JsonDocument.Parse(data);
                    root = document.RootElement;
                }
                catch (JsonException)
                {
                    continue;
                }

                // OpenAI's response typically contains a "choices" array.
                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];

                    // For chat completion endpoints, the new token is in the "delta" object.
                    if (firstChoice.TryGetProperty("delta", out var delta) &&
                        delta.TryGetProperty("content", out var contentElement))
                    {
                        var chunk = contentElement.GetString() ?? string.Empty;
                        yield return new AiRawResponse { Chunk = chunk };
                    }
                    // For the non-chat completion endpoints, the content might be at a different location.
                    else if (firstChoice.TryGetProperty("text", out var textElement))
                    {
                        var chunk = textElement.GetString() ?? string.Empty;
                        yield return new AiRawResponse { Chunk = chunk };
                    }
                }
            }
        }
    }
}