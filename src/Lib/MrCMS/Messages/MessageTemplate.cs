using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Messages
{
    public abstract class MessageTemplate
    {
        [Required, DisplayName("From Address")]
        public string FromAddress { get; set; }

        [Required, DisplayName("From Name")]
        public string FromName { get; set; }

        [Required, DisplayName("To Address")]
        public string ToAddress { get; set; }
        
        [Required, DisplayName("Replay To")]
        public string ReplayTo { get; set; }

        [DisplayName("To Name")]
        public string ToName { get; set; }

        public string Cc { get; set; }
        public string Bcc { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required, DisplayName("Is HTML?")]
        public bool IsHtml { get; set; }

        [Required, DisplayName("Is disabled?")]
        public bool IsDisabled { get; set; }

        public virtual Type ModelType => null;

        public int? SiteId { get; set; }
    }

    public abstract class MessageTemplate<T> : MessageTemplate
    {
        public override sealed Type ModelType => typeof(T);
    }
}