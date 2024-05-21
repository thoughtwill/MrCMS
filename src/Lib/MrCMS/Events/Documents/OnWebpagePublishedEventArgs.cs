using MrCMS.Entities.Documents.Web;

namespace MrCMS.Events.Documents
{
    public class OnWebpagePublishedEventArgs
    {
        public OnWebpagePublishedEventArgs(Webpage webpage)
        {
            Webpage = webpage;
        }

        public Webpage Webpage { get; set; }
    }
}