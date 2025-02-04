using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Settings;
using OpenAI.Chat;

namespace MrCMS.AI.Services.Providers.Text;

public class OpenAiTextProvider : IAiTextProvider
{
    private readonly OpenAiSettings _settings;

    public OpenAiTextProvider(OpenAiSettings settings)
    {
        _settings = settings;
    }

    public async IAsyncEnumerable<AiTextRawResponse> StreamResponseAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(
                "You are a helpful assistant integrated into a CMS. Always respond in the requested format."),
            ChatMessage.CreateUserMessage(prompt)
        };

        ChatClient client = new(model: _settings.TextModel, apiKey: _settings.ApiKey);
        var completionUpdates = client.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken);

        await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
        {
            if (completionUpdate.ContentUpdate.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(completionUpdate.ContentUpdate[0].Text))
                {
                    yield return new AiTextRawResponse { Chunk = completionUpdate.ContentUpdate[0].Text };
                }
            }
        }
    }
}