using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MrCMS.AI;
using MrCMS.AI.Models;

namespace MrCMS.Web.Admin.Models.Prompts;

public class AddPromptModel
{
    [Required, MaxLength(100),Remote("IsUniqueName", "Prompt", AdditionalFields = "Type")]
    public string Name { get; set; }
    
    [Required]
    public string Template { get; set; }
    
    [Required]
    public PromptType Type { get; set; }
}