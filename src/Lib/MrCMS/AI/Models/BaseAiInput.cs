using JetBrains.Annotations;

namespace MrCMS.AI.Models;

public class BaseAiInput
{
    [CanBeNull] public string Prompt { get; set; }
}