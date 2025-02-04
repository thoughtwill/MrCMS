using System.ComponentModel.DataAnnotations;

namespace MrCMS.AI.Models;

public class GenerateBlocksInput : BaseAiInput
{
    [Required] public int ContentVersionId { get; set; }
}