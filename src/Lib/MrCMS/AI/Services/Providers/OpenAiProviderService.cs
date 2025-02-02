using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
        // Prepare the request body as a JSON object
        var requestBody = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = true // Enable streaming
        };

        // Serialize the request body to JSON
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Add the API key to the headers
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        // Prepare the HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
        {
            Content = jsonContent
        };

        // Send the request and stream the response
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Read the response stream line by line
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            // OpenAI streams responses in a specific format, so we need to parse it
            if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data: "))
            {
                var data = line.Substring("data: ".Length).Trim();

                // Skip empty or "[DONE]" messages
                if (data == "[DONE]")
                    continue;

                yield return new AiRawResponse { Chunk = data };
            }
        }
    }
}