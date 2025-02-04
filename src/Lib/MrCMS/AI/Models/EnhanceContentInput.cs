using System.ComponentModel.DataAnnotations;

namespace MrCMS.AI.Models;

public class EnhanceContentInput : BaseAiInput
{
    [Required]
    public int WebpageId { get; set; }
    
  
}