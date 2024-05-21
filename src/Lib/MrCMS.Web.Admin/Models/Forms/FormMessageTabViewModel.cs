using System.ComponentModel;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormMessageTabViewModel
    {
        public int Id { get; set; }
        public string FormSubmittedMessage { get; set; }
        public string FormRedirectUrl { get; set; }
        public string SubmitButtonText { get; set; }
        public string SubmitButtonCssClass { get; set; }
        [DisplayName("Subject")]
        public string FormEmailTitle { get; set; }
        [DisplayName("To")]
        public string SendFormTo { get; set; }
        
        [DisplayName("Reply To")]
        public string FormReplyTo { get; set; }
        
        [DisplayName("Message")]
        public string FormMessage { get; set; }
    }
}