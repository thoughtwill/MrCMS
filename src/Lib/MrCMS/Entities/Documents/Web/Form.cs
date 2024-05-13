using MrCMS.Entities.Documents.Web.FormProperties;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Entities.Documents.Web
{
    public class Form : SiteEntity
    {
        public Form()
        {
            FormPostings = new List<FormPosting>();
            FormProperties = new List<FormProperty>();
        }
        [Required]
        [StringLength(100)]
        public virtual string Name { get; set; }

        [DisplayName("Form Submitted Message")]
        [StringLength(500, ErrorMessage = "Form submitted message cannot be longer than 500 characters.")]
        public virtual string FormSubmittedMessage { get; set; }

        [DisplayName("Form Success Redirect")]
        [StringLength(500)]
        public virtual string FormRedirectUrl { get; set; }

        [DisplayName("Subject")]
        [StringLength(250, ErrorMessage = "Subject cannot be longer than 250 characters.")]
        public virtual string FormEmailTitle { get; set; }

        [DisplayName("Send Form To")]
        [StringLength(500, ErrorMessage = "Send to cannot be longer than 500 characters.")]
        public virtual string SendFormTo { get; set; }
        
        [DisplayName("Reply To")]
        [StringLength(500, ErrorMessage = "Send to cannot be longer than 500 characters.")]
        public virtual string FormReplyTo { get; set; }

        [DisplayName("Form Email Message")]
        public virtual string FormMessage { get; set; }

        [StringLength(100)]
        [DisplayName("Submit Button Css Class")]
        public virtual string SubmitButtonCssClass { get; set; }

        [StringLength(100)]
        [DisplayName("Submit button custom text")]
        public virtual string SubmitButtonText { get; set; }

        public virtual IList<FormProperty> FormProperties { get; set; }

        public virtual IList<FormPosting> FormPostings { get; set; }

        public virtual string FormDesign { get; set; }



        [DisplayName("Delete entries after (days)")]
        public virtual int? DeleteEntriesAfter { get; set; }

        public virtual bool SendByEmailOnly { get; set; }

        public virtual bool ShowGDPRConsentBox { get; set; }
    }
}