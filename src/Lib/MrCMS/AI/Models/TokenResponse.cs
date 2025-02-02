namespace MrCMS.AI.Models;

/// <summary>
/// The generic response model that contains the token name and its associated content.
/// </summary>
public class TokenResponse
{
    public string Token { get; set; }
    public string Content { get; set; }
}