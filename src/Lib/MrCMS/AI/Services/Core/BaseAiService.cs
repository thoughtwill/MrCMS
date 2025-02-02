using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.AI.Models;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Core;

/// <summary>
/// The base AI service that processes a stream of text from the AI provider and yields token responses.
/// 
/// The tokens (for example "title", "description", "keywords") are defined by the derived class.
/// The derived class also supplies the full prompt via the Prompt property.
/// 
/// The implementation below uses a state machine so that even if token markers (or token content)
/// are split across chunks, we can “remember” what we’re processing. Moreover, while inside a token,
/// we yield partial token content as soon as a chunk is processed.
/// 
/// HTML tags that do not match one of the expected token markers are preserved in the token content.
/// </summary>
public abstract class BaseAiService
{
    protected readonly AiSettings Settings;
    protected readonly IAiProvider Provider;

    protected BaseAiService(IServiceProvider serviceProvider)
    {
        Settings = serviceProvider.GetRequiredService<AiSettings>();
        var aiProviderFactory = serviceProvider.GetRequiredService<AiProviderFactory>();
        Provider = aiProviderFactory.GetProvider();
    }
    
    /// <summary>
    /// The complete prompt to send to the AI provider. Derived classes override this.
    /// </summary>
    protected abstract string Prompt { get; }

    /// <summary>
    /// The tokens expected in the response (for example: "title", "description", "keywords").
    /// Derived classes override this.
    /// </summary>
    protected abstract IEnumerable<string> TokenNames { get; }

    // The states in our state machine.
    private enum ProcessingState
    {
        /// <summary>
        /// We have not yet encountered a token marker.
        /// </summary>
        Outside,

        /// <summary>
        /// We are reading a marker (opening or closing) but haven’t seen the closing '>'.
        /// </summary>
        InMarker,

        /// <summary>
        /// We are inside a token’s content.
        /// </summary>
        InsideToken
    }
    
    /// <summary>
    /// Processes the streamed response from the AI provider. The code “remembers” any incomplete data
    /// between chunks and uses a state machine to detect token markers. When inside a token, as soon as a
    /// chunk is done we yield the partial content.
    /// </summary>
    /// <returns>An asynchronous stream of <see cref="TokenResponse"/>.</returns>
    protected async IAsyncEnumerable<TokenResponse> ProcessStreamAsync()
    {
        // This state will be maintained across chunks.
        var state = ProcessingState.Outside;

        // When reading a marker (for example "<tit" followed by "le>"), we accumulate its characters.
        var markerBuffer = new StringBuilder();

        // When inside a token (after a recognized opening marker), we accumulate text until a marker is encountered.
        var contentBuffer = new StringBuilder();

        // This holds any leftover data that wasn’t processed completely at the end of a chunk.
        var leftover = string.Empty;

        // When a token is opened, store its name here.
        string currentToken = null;

        // Loop over each chunk from the provider.
        await foreach (var rawResponse in Provider.StreamResponseAsync(Prompt))
        {
            // Prepend any leftover from previous chunk.
            var input = leftover + rawResponse.Chunk;
            leftover = string.Empty; // reset

            // Process character‐by‐character.
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                switch (state)
                {
                    case ProcessingState.Outside:
                        if (c == '<')
                        {
                            // Possibly the start of an opening marker.
                            state = ProcessingState.InMarker;
                            markerBuffer.Clear();
                            markerBuffer.Append(c);
                        }
                        // Ignore characters outside any marker.
                        break;

                    case ProcessingState.InMarker:
                        markerBuffer.Append(c);
                        if (c == '>')
                        {
                            // We have a complete marker.
                            var marker = markerBuffer.ToString();

                            // Only if we're inside a token do we need to decide whether this marker is a token marker or HTML.
                            if (currentToken != null)
                            {
                                // Determine if this marker is one of our expected token markers.
                                bool isExpected = false;
                                bool isClosing = false;
                                string tokenNameFromMarker = null;

                                if (marker.StartsWith("</", StringComparison.OrdinalIgnoreCase))
                                {
                                    tokenNameFromMarker = marker.Substring(2, marker.Length - 3).Trim();
                                    isClosing = true;
                                }
                                else if (marker.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                                {
                                    tokenNameFromMarker = marker.Substring(1, marker.Length - 2).Trim();
                                }

                                if (!string.IsNullOrEmpty(tokenNameFromMarker))
                                {
                                    foreach (var t in TokenNames)
                                    {
                                        if (string.Equals(tokenNameFromMarker, t, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isExpected = true;
                                            break;
                                        }
                                    }
                                }

                                if (isExpected)
                                {
                                    // This marker is one of our expected token markers.
                                    if (isClosing && string.Equals(tokenNameFromMarker, currentToken, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // This is a closing marker for the current token.
                                        if (contentBuffer.Length > 0)
                                        {
                                            yield return new TokenResponse
                                            {
                                                Token = currentToken,
                                                Content = contentBuffer.ToString()
                                            };
                                            contentBuffer.Clear();
                                        }
                                        currentToken = null;
                                        state = ProcessingState.Outside;
                                    }
                                    else if (!isClosing)
                                    {
                                        // If we encounter an opening marker while already inside a token, we treat it as a new token.
                                        // (Depending on your needs you might decide to ignore nested tokens.)
                                        currentToken = tokenNameFromMarker;
                                        state = ProcessingState.InsideToken;
                                    }
                                    else
                                    {
                                        // Unlikely: a closing marker that doesn't match current token.
                                        state = ProcessingState.Outside;
                                    }
                                }
                                else
                                {
                                    // Not an expected token marker, so treat the marker as literal HTML.
                                    contentBuffer.Append(marker);
                                    state = ProcessingState.InsideToken;
                                }
                            }
                            else
                            {
                                // We are outside any token. Check if this is a valid opening marker.
                                bool isExpected = false;
                                string tokenNameFromMarker = null;
                                if (marker.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                                {
                                    tokenNameFromMarker = marker.Substring(1, marker.Length - 2).Trim();
                                    foreach (var t in TokenNames)
                                    {
                                        if (string.Equals(tokenNameFromMarker, t, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isExpected = true;
                                            break;
                                        }
                                    }
                                }
                                if (isExpected)
                                {
                                    // Recognized opening marker; enter token content state.
                                    currentToken = tokenNameFromMarker;
                                    state = ProcessingState.InsideToken;
                                }
                                // Otherwise ignore the marker (or you could choose to do something else).
                                markerBuffer.Clear();
                            }
                        }
                        // If '>' is not yet encountered, we continue accumulating in markerBuffer.
                        break;

                    case ProcessingState.InsideToken:
                        if (c == '<')
                        {
                            // We encountered a '<' while inside token content.
                            // Instead of immediately switching out, we now enter marker reading,
                            // but we do not yield the partial content yet.
                            state = ProcessingState.InMarker;
                            markerBuffer.Clear();
                            markerBuffer.Append(c);
                        }
                        else
                        {
                            // Regular content inside a token.
                            contentBuffer.Append(c);
                        }
                        break;
                }
            } // end for each character

            // At the end of the chunk, if we are in the middle of a marker, save it.
            if (state == ProcessingState.InMarker)
            {
                leftover = markerBuffer.ToString();
                markerBuffer.Clear();
            }
            else
            {
                leftover = string.Empty;
            }

            // Yield any accumulated content if still inside a token.
            if (state == ProcessingState.InsideToken && contentBuffer.Length > 0)
            {
                yield return new TokenResponse
                {
                    Token = currentToken,
                    Content = contentBuffer.ToString()
                };
                contentBuffer.Clear();
            }
            // (If we’re Outside, any stray text is ignored.)
        } // end foreach chunk

        // After the stream ends, if we’re still inside a token and have pending content, yield it.
        if (state == ProcessingState.InsideToken && contentBuffer.Length > 0)
        {
            yield return new TokenResponse
            {
                Token = currentToken,
                Content = contentBuffer.ToString()
            };
        }
    }
}