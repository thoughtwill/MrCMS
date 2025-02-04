using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace MrCMS.AI.Models;

public class GenerateSeoInput
{
    [Required] public int WebpageId { get; set; }

    [CanBeNull] public string Prompt { get; set; }
}