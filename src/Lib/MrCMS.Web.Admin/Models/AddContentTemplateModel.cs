using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MrCMS.Web.Admin.Models;

public class AddContentTemplateModel
{
    [Required,Remote("IsUniqueName", "ContentTemplate", AdditionalFields = "Id")] public string Name { get; set; }
    
    public string Text { get; set; }
}