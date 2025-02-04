using System.ComponentModel.DataAnnotations;
using MrCMS.AI;
using MrCMS.AI.Models;

namespace MrCMS.Web.Admin.Models.Prompts;

public class PromptSearchModel
{
    public int Page { get; set; } = 1;
    public string Name { get; set; }
    
    [Required]
    public PromptType Type { get; set; } = PromptType.TextPrompt;
}