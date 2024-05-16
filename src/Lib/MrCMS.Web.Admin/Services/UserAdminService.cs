using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.People;
using MrCMS.Mapping;
using MrCMS.Services;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserManagementService _service;
        private readonly ISessionAwareMapper _mapper;
        private readonly IPasswordManagementService _passwordManagementService;
        private readonly IRoleService _roleService;

        public UserAdminService(IUserManagementService service, ISessionAwareMapper mapper,
            IPasswordManagementService passwordManagementService, IRoleService roleService)
        {
            _service = service;
            _mapper = mapper;
            _passwordManagementService = passwordManagementService;
            _roleService = roleService;
        }

        public async Task<int> AddUser(AddUserModel addUserModel)
        {
            var user = new User();
            var mappedUser = _mapper.Map(addUserModel, user);
            await _service.AddUser(mappedUser);
            _passwordManagementService.SetPassword(mappedUser, addUserModel.Password, addUserModel.ConfirmPassword);
            await _service.AddUser(mappedUser);
            return mappedUser.Id;
        }

        public UpdateUserModel GetUpdateModel(User user)
        {
            return _mapper.Map<UpdateUserModel>(user);
        }

        public async Task<User> SaveUser(UpdateUserModel model, List<int> roles)
        {
            var user = await _service.GetUser(model.Id);
            _mapper.Map(model, user);

            HashSet<UserRole> rolesToSet = new HashSet<UserRole>();
            foreach (var id in roles)
            {
                var role = await _roleService.GetRole(id);
                if (role != null)
                    rolesToSet.Add(role);
            }

            user.Roles = rolesToSet;
            await _service.SaveUser(user);
            return user;
        }

        public async Task DeleteUser(int id)
        {
            await _service.DeleteUser(id);
        }

        public async Task<User> GetUser(int id)
        {
            return await _service.GetUser(id);
        }

        public async Task<bool> IsUniqueEmail(string email, int? id)
        {
            return await _service.IsUniqueEmail(email, id);
        }

        public async Task SetPassword(int id, string password)
        {
            var user = await _service.GetUser(id);
            _passwordManagementService.SetPassword(user, password, password);
            await _service.SaveUser(user);
        }
    }
}