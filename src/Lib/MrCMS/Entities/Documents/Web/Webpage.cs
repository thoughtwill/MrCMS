using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MrCMS.Entities.People;
using MrCMS.Helpers;

namespace MrCMS.Entities.Documents.Web
{
    public abstract class Webpage : SiteEntity
    {
        public virtual string WebpageTypeName { get; }


        public enum WebpagePublishStatus
        {
            Published,
            Scheduled,
            Unpublished
        }

        protected Webpage()
        {
            IncludeInSitemap = true;
            Urls = new List<UrlHistory>();
            FrontEndAllowedRoles = new HashSet<UserRole>();
            ContentVersions = new List<ContentVersion>();
            Versions = new List<WebpageVersion>();
            Tags = new HashSet<Tag>();
        }
        [Required] [StringLength(255)] public virtual string Name { get; set; }
        public virtual Webpage Parent { get; set; }

        [Required] public virtual string UrlSegment { get; set; }
        
        [Required]
        [DisplayName("Display Order")]
        public virtual int DisplayOrder { get; set; }

        public virtual bool HideInAdminNav { get; set; }
        
        [StringLength(250)] public virtual string SEOTargetPhrase { get; set; }

        [StringLength(250)] public virtual string MetaTitle { get; set; }

        [StringLength(250)] public virtual string MetaDescription { get; set; }

        [StringLength(250)] public virtual string MetaKeywords { get; set; }

        public virtual string ExplicitCanonicalLink { get; set; }

        public virtual bool RevealInNavigation { get; set; }

        public virtual bool IncludeInSitemap { get; set; }

        [StringLength(8000)] public virtual string CustomHeaderScripts { get; set; }

        [StringLength(8000)] public virtual string CustomFooterScripts { get; set; }

        public virtual bool Published { get; set; }

        public virtual WebpagePublishStatus PublishStatus
        {
            get
            {
                WebpagePublishStatus status = Published
                    ? WebpagePublishStatus.Published
                    : PublishOn.HasValue
                        ? WebpagePublishStatus.Scheduled
                        : WebpagePublishStatus.Unpublished;
                return status;
            }
        }

        public virtual DateTime? PublishOn { get; set; }

        public virtual string BodyContent { get; set; }

        public virtual IEnumerable<Webpage> ActivePages
        {
            get
            {
                Webpage page = this;
                while (page != null)
                {
                    yield return page;
                    page = page.Parent.Unproxy() as Webpage;
                }
            }
        }

        [DisplayName("Block Anonymous Access")]
        public virtual bool BlockAnonymousAccess { get; set; }

        public virtual bool HasCustomPermissions { get; set; }
        public virtual WebpagePermissionType PermissionType { get; set; }

        public virtual ISet<UserRole> FrontEndAllowedRoles { get; set; }
        public virtual string Password { get; set; }
        public virtual Guid? PasswordAccessToken { get; set; }

        [DisplayName("Roles")]
        public virtual string FrontEndRoles
        {
            get { return string.Join(", ", FrontEndAllowedRoles.Select(x => x.Name)); }
        }

        public virtual IList<UrlHistory> Urls { get; set; }

        public virtual PageTemplate PageTemplate { get; set; }


        [DisplayName("Do not cache?")] public virtual bool DoNotCache { get; set; }

        public virtual IList<ContentVersion> ContentVersions { get; set; }

        public virtual ContentVersion LiveContentVersion =>
            ContentVersions.OrderByDescending(x => x.CreatedOn).FirstOrDefault(x => x.IsLive);
        
        public virtual ISet<Tag> Tags { get; set; }

        public virtual string TagList
        {
            get { return string.Join(",", Tags.Select(x => x.Name)); }
        }
        public virtual string WebpageType => GetType().Name;

        protected internal virtual IList<WebpageVersion> Versions { get; set; }
        public virtual ISet<TagPage> TagPages { get; set; }

        public virtual string TagPageList
        {
            get { return string.Join(",", TagPages?.Select(x => x.Name) ?? new List<string>()); }
        }
    }
}