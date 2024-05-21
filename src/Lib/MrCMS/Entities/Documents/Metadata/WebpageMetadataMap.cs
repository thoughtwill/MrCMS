using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;

namespace MrCMS.Entities.Documents.Metadata
{
    /// <summary>
    /// Abstract class useed to define metadata of your documents within Mr CMS
    /// </summary>
    /// <typeparam name="T">WebpageType</typeparam>
    public abstract class WebpageMetadataMap<T> : IGetWebpageMetadataInfo where T : Webpage, new()
    {
        /// <summary>
        /// Name of the document type for display when adding a page in Mr CMS. I.E Text Page, Blog Page etc
        /// </summary>
        public virtual string Name => typeof(T).Name.BreakUpString();

        /// <summary>
        /// The icon shown next to the page type in Mr CMS admin. By default icons are taken from bootstrap
        /// </summary>
        public virtual string IconClass => "fa fa-file";

        /// <summary>
        /// Controller used to render this pagetype when a GET/HEAD request is made. By Default is 'Webpage'
        /// </summary>
        public virtual string WebGetController => "Webpage";

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a GET request is made. By Default is 'Show'
        /// </summary>
        public virtual string WebGetAction => "Show";
        
        /// <summary>
        /// Controller used to render this pagetype when a GET/HEAD request is made and the user is unauthorised. By Default is 'Error'
        /// </summary>
        public virtual string WebGetControllerUnauthorized => "Error";

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a GET request is made and the user is unauthorized. By Default is 'Handle401'
        /// </summary>
        public virtual string WebGetActionUnauthorized => "Handle401";
        
        /// <summary>
        /// Controller used to render this pagetype when a GET/HEAD request is made and the user is forbidden. By Default is 'Error'
        /// </summary>
        public virtual string WebGetControllerForbidden => "Error";

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a GET request is made and the user is forbidden. By Default is 'Handle403'
        /// </summary>
        public virtual string WebGetActionForbidden => "Handle403";

        /// <summary>
        /// Controller used to render this pagetype when a POST request is made. By convention is the name of the Document Controller.
        /// </summary>
        public virtual string WebPostController => typeof(T).Name;

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a POST request is made. By Default is 'Post'
        /// </summary>
        public virtual string WebPostAction => "Post";
        
        /// <summary>
        /// Controller used to render this pagetype when a POST request is made and the user is unauthorised. By Default is 'Error'
        /// </summary>
        public virtual string WebPostControllerUnauthorized => "Error";

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a POST request is made and the user is unauthorized. By Default is 'Handle401'
        /// </summary>
        public virtual string WebPostActionUnauthorized => "Handle401";
        
        /// <summary>
        /// Controller used to render this pagetype when a POST request is made and the user is forbidden. By Default is 'Error'
        /// </summary>
        public virtual string WebPostControllerForbidden => "Error";

        /// <summary>
        /// Action method on specified controller used to render this pagetype when a POST request is made and the user is forbidden. By Default is 'Handle403'
        /// </summary>
        public virtual string WebPostActionForbidden => "Handle403";

        /// <summary>
        /// For advanced use where complex processes are required. Take an eCommerce payment system for example.
        /// You need PaymentInfo and other models to be passed to the action methods on contollers. 
        /// Use Post Types to define these objects. Alternatively use FormCollection if strong typing is not required.
        /// </summary>
        public virtual Type[] PostTypes => new Type[0];

        /// <summary>
        /// Used to overide display order by another document property. I.E display by document date.
        /// </summary>
        public virtual SortBy SortBy => SortBy.DisplayOrder;

        /// <summary>
        /// Determines whether children of this document type are sortable
        /// </summary>
        public virtual bool Sortable => true;

        /// <summary>
        /// Number of child notdse to show in web tree within admin. Usefull if there are to be many document nested I.E News / Blog etc
        /// </summary>
        public virtual int MaxChildNodes => 50;

        /// <summary>
        /// Display order in admin when adding a page
        /// </summary>
        public virtual int DisplayOrder => 1;

        /// <summary>
        /// System type
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Determines the allowed children for the Document. 
        /// </summary>
        public virtual ChildrenListType ChildrenListType => ChildrenListType.BlackList;

        /// <summary>
        /// If ChildrenListType is BlackList, any Types passed in here will not be allowed to be added to this Document Type. And vice-versa, if ChildrenListType is Whitelist, Types passed in ChildrenList will be allowed only.
        /// </summary>
        public virtual IEnumerable<Type> ChildrenList => Enumerable.Empty<Type>();

        /// <summary>
        /// If set to true, type must be added to white list to be a child.
        /// </summary>
        public virtual bool AutoBlacklist => false;

        /// <summary>
        /// Specifies if the document can be a route page. For example a blog page would need to be underneath a blog container and this value would need to be true.
        /// </summary>
        public virtual bool RequiresParent => false;

        /// <summary>
        /// Specifies the default layout for the document type to save having to select the layout each time a page is added. 
        /// </summary>
        public virtual string DefaultLayoutName => "Default Layout";

        /// <summary>
        /// Specifies the partial view used to render the View
        /// </summary>
        public virtual string EditPartialView => "DefaultEdit";

        public virtual Type EditModel => null;

        /// <summary>
        /// Specifies whether the children of the page are shown in the navigation tree
        /// </summary>
        public virtual bool ShowChildrenInAdminNav => true;

        /// <summary>
        /// Specifies whether child pages will have Maintain Hierarchy checked by default
        /// </summary>
        public virtual bool ChildrenMaintainHierarchy => true;

        /// <summary>
        /// Specifies whether child pages will have Maintain Hierarchy checked by default
        /// </summary>
        public virtual bool RevealInNavigation => true;

        /// <summary>
        /// Specifies whether the page type is able to have body content
        /// </summary>
        public virtual bool HasBodyContent => true;

        public WebpageMetadataInfo Metadata =>
            new WebpageMetadataInfo
            {
                Name = Name,
                IconClass = IconClass,
                WebGetAction = WebGetAction,
                WebGetController = WebGetController,
                WebPostAction = WebPostAction,
                WebPostController = WebPostController,
                WebGetActionUnauthorized = WebGetActionUnauthorized,
                WebGetControllerUnauthorized = WebGetControllerUnauthorized,
                WebPostActionUnauthorized = WebPostActionUnauthorized,
                WebPostControllerUnauthorized = WebPostControllerUnauthorized,
                WebGetActionForbidden = WebGetActionForbidden,
                WebGetControllerForbidden = WebGetControllerForbidden,
                WebPostActionForbidden = WebPostActionForbidden,
                WebPostControllerForbidden = WebPostControllerForbidden,
                SortBy = SortBy,
                MaxChildNodes = MaxChildNodes,
                Sortable = Sortable,
                DisplayOrder = DisplayOrder,
                Type = Type,
                PostTypes = PostTypes,
                ChildrenListType = ChildrenListType,
                ChildrenList = ChildrenList,
                AutoBlacklist = AutoBlacklist,
                RequiresParent = RequiresParent,
                DefaultLayoutName = DefaultLayoutName,
                EditPartialView = EditPartialView,
                ShowChildrenInAdminNav = ShowChildrenInAdminNav,
                ChildrenMaintainHierarchy = ChildrenMaintainHierarchy,
                RevealInNavigation = RevealInNavigation,
                HasBodyContent = HasBodyContent
            };
    }
}