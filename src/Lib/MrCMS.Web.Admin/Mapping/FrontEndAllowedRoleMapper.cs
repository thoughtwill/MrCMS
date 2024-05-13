using System.Collections.Generic;
using AutoMapper;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.People;
using MrCMS.Web.Admin.Models.WebpageEdit;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Mapping
{
    public class FrontEndAllowedRoleMapper : IValueResolver<PermissionsTabViewModel, Webpage, ISet<UserRole>>
    {
        private readonly IWebpageRolesAdminService _webpageRolesAdminService;

        public FrontEndAllowedRoleMapper(IWebpageRolesAdminService webpageRolesAdminService)
        {
            _webpageRolesAdminService = webpageRolesAdminService;
        }

        public ISet<UserRole> Resolve(PermissionsTabViewModel source, Webpage destination, ISet<UserRole> destMember, ResolutionContext context)
        {
            // if should not be set
            if (!source.HasCustomPermissions || (source.PermissionType != WebpagePermissionType.RoleBased && source.PermissionType != WebpagePermissionType.RelatedContentRoleBased))
            {
                // return empty collection
                return new HashSet<UserRole>();
            }
            return _webpageRolesAdminService.GetFrontEndRoles(source.FrontEndRoles);
        }
    }
}