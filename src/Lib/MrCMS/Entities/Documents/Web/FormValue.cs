namespace MrCMS.Entities.Documents.Web
{
    public class FormValue : SiteEntity
    {
        public virtual FormPosting FormPosting { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }
        public virtual bool IsFile { get; set; }

        public virtual string GetMessageValue()
        {
            return IsFile
                ? $"<a href=\"{(Value.StartsWith("http") ? Value : $"http://{Site.BaseUrl}{Value}")}\">{Value}</a>"
                : Value;
        }
    }
}