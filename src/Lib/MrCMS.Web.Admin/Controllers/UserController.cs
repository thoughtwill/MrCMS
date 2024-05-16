using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.Auth;
using MrCMS.Web.Admin.ModelBinders;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Website;

namespace MrCMS.Web.Admin.Controllers
{
    public class UserController : MrCMSAdminController
    {
        private readonly IUserAdminService _userAdminService;
        private readonly IUserSearchService _userSearchService;
        private readonly IRoleService _roleService;
        private readonly IGetUserCultureOptions _getUserCultureOptions;
        private readonly IUserImpersonationService _userImpersonationService;
        private readonly UserManager _userManager;
        private readonly ISignInManager _signInManager;

        public UserController(IUserAdminService userAdminService, IUserSearchService userSearchService,
            IRoleService roleService,
            IGetUserCultureOptions getUserCultureOptions, IUserImpersonationService userImpersonationService,
            UserManager userManager,
            ISignInManager signInManager)
        {
            _userAdminService = userAdminService;
            _userSearchService = userSearchService;
            _roleService = roleService;
            _getUserCultureOptions = getUserCultureOptions;
            _userImpersonationService = userImpersonationService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Acl(typeof(UserACL), UserACL.View)]
        public async Task<ActionResult> Index(UserSearchQuery searchQuery)
        {
            ViewData["users"] = await _userSearchService.GetUsersPaged(searchQuery);
            ViewData["roles"] = await _userSearchService.GetAllRoleOptions();
            return View(searchQuery);
        }

        [HttpGet]
        [Acl(typeof(UserACL), UserACL.Add)]
        public PartialViewResult Add()
        {
            var model = new AddUserModel();
            ViewData["culture-options"] = _getUserCultureOptions.Get();
            return PartialView(model);
        }

        [HttpPost]
        [Acl(typeof(UserACL), UserACL.Add)]
        public async Task<RedirectToActionResult> Add(AddUserModel addUserModel)
        {
            var addUser = await _userAdminService.AddUser(addUserModel);

            return RedirectToAction("Edit", new {id = addUser});
        }

        [HttpGet]
        [ActionName("Edit")]
        [Acl(typeof(UserACL), UserACL.Edit)]
        public async Task<ActionResult> Edit_Get(int id)
        {
            var user = await _userAdminService.GetUser(id);
            if (user == null)
                return RedirectToAction("Index");
            ViewData["AvailableRoles"] = await _roleService.GetAllRoles();
            ViewData["OnlyAdmin"] = await _roleService.IsOnlyAdmin(user);
            ViewData["culture-options"] = _getUserCultureOptions.Get();
            ViewData["user"] = user;

            var updateUserModel = _userAdminService.GetUpdateModel(user);
            return View(updateUserModel);
        }

        [HttpPost]
        [Acl(typeof(UserACL), UserACL.Edit)]
        public async Task<RedirectToActionResult> Edit(UpdateUserModel model,
            [ModelBinder(typeof(UpdateUserRoleModelBinder))]
            List<int> roles)
        {
            var user = await _userAdminService.SaveUser(model, roles);
            TempData.AddSuccessMessage($"{user.Name} successfully saved");
            return RedirectToAction("Edit", "User", new {Id = user.Id});
        }

        [HttpGet]
        [ActionName("Delete")]
        [Acl(typeof(UserACL), UserACL.Delete)]
        public async Task<PartialViewResult> Delete_Get(int id)
        {
            var user = await _userAdminService.GetUser(id);
            return PartialView(user);
        }

        [HttpPost]
        [Acl(typeof(UserACL), UserACL.Delete)]
        public async Task<RedirectToActionResult> Delete(int id)
        {
            await _userAdminService.DeleteUser(id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Acl(typeof(UserACL), UserACL.SetPassword)]
        public async Task<ActionResult> SetPassword(int id)
        {
            return PartialView(await _userAdminService.GetUser(id));
        }

        [HttpPost]
        [Acl(typeof(UserACL), UserACL.SetPassword)]
        public async Task<RedirectToActionResult> SetPassword(int id, string password)
        {
            await _userAdminService.SetPassword(id, password);
            return RedirectToAction("Edit", new {id});
        }

        public async Task<JsonResult> IsUniqueEmail(string email, int? id)
        {
            if (await _userAdminService.IsUniqueEmail(email, id))
            {
                return Json(true);
            }

            return Json("Email already registered.");
        }

        public async Task<IActionResult> Impersonate(int id)
        {
            var user = await _userAdminService.GetUser(id);
            var result = await _userImpersonationService.Impersonate(User, user);
            if (result.Success)
            {
                // refresh the user to get the new claims
                await _signInManager.RefreshSignInAsync(result.UnderlyingUser);
                return Redirect("~/");
            }

            TempData.AddErrorMessage(result.Error);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ResetSecurityStamp(int id)
        {
            var user = await _userAdminService.GetUser(id);
            var result = await _userManager.UpdateSecurityStampAsync(user);
            TempData.AddSuccessMessage("Security Stamp Updated. User will be logged out in the next few minutes.");
            return RedirectToAction("Edit", new {id});
        }
    }
}