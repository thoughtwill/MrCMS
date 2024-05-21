using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Website.Filters;

namespace MrCMS.Website.Controllers
{
    public class FormController : MrCMSUIController
    {
        private readonly IFormPostingHandler _formPostingHandler;

        public FormController(IFormPostingHandler formPostingHandler)
        {
            _formPostingHandler = formPostingHandler;
        }

        [GoogleRecaptcha]
        [Route("save-form/{id}")]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Save(int id)
        {
            var form = _formPostingHandler.GetForm(id);
            if (form?.IsDeleted != false)
                return new EmptyResult();
            var saveFormData = await _formPostingHandler.SaveFormData(form, Request);

            TempData["form-submitted"] = true;
            TempData.Set(saveFormData, "form-submitted-message");
            // if any errors add form data to be renderered, otherwise form should be empty
            TempData.Set(saveFormData.Any() ? Request.Form : null, "form-data");

        

            var redirectUrl = (string)Request.Form["returnUrl"] ?? "/";
            if (!saveFormData.Any() && !string.IsNullOrEmpty(form.FormRedirectUrl))
                redirectUrl = form.FormRedirectUrl;
            if (!string.IsNullOrWhiteSpace(redirectUrl))
                return Redirect(redirectUrl);
            return Redirect("/");
        }
    }
}