using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Web.Admin.Models
{
    public class AddMediaCategoryModel
    {
        public int? ParentId { get; set; }

        [Required]
        [DisplayName("Folder Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Path")]
        public string Path { get; set; }
    }
}