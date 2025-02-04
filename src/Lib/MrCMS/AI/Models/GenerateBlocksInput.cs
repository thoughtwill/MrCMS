using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace MrCMS.AI.Models;

public class GenerateBlocksInput
{
    [Required] public int ContentVersionId { get; set; }

    [CanBeNull] public string Prompt { get; set; }
}