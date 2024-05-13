using MrCMS.Entities.Documents.Web;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Web.Apps.Core.Entities.BlockItems
{
    [Display(Name = "Embed")]
    public class EmbedVideo : BlockItem
    {
        public string Poster { get; set; }
        public string MobilePoster { get; set; }
        public string EmbedCode { get; set; }
    }
}