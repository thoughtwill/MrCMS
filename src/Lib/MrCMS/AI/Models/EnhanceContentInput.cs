using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace MrCMS.AI.Models;

public class EnhanceContentInput
{
    [Required]
    public int WebpageId { get; set; }
    
    [CanBeNull] public string Prompt { get; set; }
}