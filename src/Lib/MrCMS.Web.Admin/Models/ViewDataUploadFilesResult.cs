using MrCMS.Entities.Documents.Media;
using MrCMS.Helpers;

namespace MrCMS.Web.Admin.Models
{
    public class ViewDataUploadFilesResult
    {
        public ViewDataUploadFilesResult(MediaFile file)
        {
            Id = file.Id;
            name = file.FileName;
            url = file.FileUrl;
            size = file.ContentLength;
            Type = file.ContentType;
            seo_url = $"/admin/File/UpdateSEO/{file.Id}";
            title = file.Title ?? string.Empty;
            description = file.Description ?? string.Empty;
            display_order = file.DisplayOrder;
            is_image = file.IsImage();
        }


        public int Id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public long size { get; set; }
        public string Type { get; set; }
        public string seo_url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int display_order { get; set; }
        public bool is_image { get; set; }
    }
}