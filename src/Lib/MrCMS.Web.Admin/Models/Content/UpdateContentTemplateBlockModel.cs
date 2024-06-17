using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Web.Admin.Models.Content;

public class UpdateContentTemplateBlockModel
{
    [Required]
    [DisplayName("Content Template")]
    public int? ContentTemplateId { get; set; } 
    
    public string Name { get; set; }
    
    public string Properties { get; set; }
}