using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.People;
using MrCMS.Mapping;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public class RoleAdminService : IRoleAdminService
    {
        private readonly IRoleService _roleService;
        private readonly ISessionAwareMapper _mapper;

        public RoleAdminService(IRoleService roleService, ISessionAwareMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserRole>> GetAllRoles()
        {
            return await _roleService.GetAllRoles();
        }

        public async Task<IEnumerable<string>> GetRolesForPermissions()
        {
            var allRoles = await _roleService.GetAllRoles();
            return allRoles.Select(x => x.Name);
        }

        public Task<IEnumerable<AutoCompleteResult>> Search(string term)
        {
            return _roleService.Search(term);
        }

        public async Task<AddRoleResult> AddRole(AddRoleModel model)
        {
            if (await _roleService.GetRoleByName(model.Name) != null)
                return new AddRoleResult(false, $"{model.Name} already exists.");
            var role = _mapper.Map<UserRole>(model);
            await _roleService.SaveRole(role);
            return new AddRoleResult(true, null);
        }

        public async Task<UpdateRoleModel> GetEditModel(int id)
        {
            return _mapper.Map<UpdateRoleModel>(await GetRole(id));
        }

        private async Task<UserRole> GetRole(int id)
        {
            return await _roleService.GetRole(id);
        }

        public async Task SaveRole(UpdateRoleModel model)
        {
            var role = await GetRole(model.Id);
            _mapper.Map(model, role);
            await _roleService.SaveRole(role);
        }

        public async Task DeleteRole(int id)
        {
            await _roleService.DeleteRole(await GetRole(id));
        }
    }
}