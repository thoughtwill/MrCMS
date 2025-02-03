using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Settings;
using OpenAI.Chat;

namespace MrCMS.AI.Services.Providers;

public class OpenAiProviderService : IAiProvider
{
    private readonly OpenAiSettings _settings;

    public OpenAiProviderService(OpenAiSettings settings)
    {
        _settings = settings;
    }

    public async IAsyncEnumerable<AiRawResponse> StreamResponseAsync(string prompt)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(
                "You are a helpful assistant integrated into a CMS. Always respond in the requested format."),
            ChatMessage.CreateUserMessage(prompt)
        };

        ChatClient client = new(model: _settings.Model, apiKey: _settings.ApiKey);
        var completionUpdates = client.CompleteChatStreamingAsync(messages);

        await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
        {
            if (completionUpdate.ContentUpdate.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(completionUpdate.ContentUpdate[0].Text))
                {
                    yield return new AiRawResponse { Chunk = completionUpdate.ContentUpdate[0].Text };
                }
            }
        }
    }
}