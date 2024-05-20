using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MrCMS.Website.Optimization;

namespace MrCMS.Web.Admin.Bundles
{
    public class AdminScriptBundle : IAdminScriptBundle
    {
        public int Priority => int.MaxValue;
        public Task<bool> ShouldShow(string theme)
        {
            return Task.FromResult(true);
        }

        public string Url => "/Areas/Admin/assets/admin.js";

        public IEnumerable<string> VendorFiles
        {
            get
            {
                yield return "/Areas/Admin/Content/lib/ckeditor/ckeditor.js";
                yield return "/Areas/Admin/Content/lib/ckeditor/config.js";
                yield return "/Areas/Admin/Content/lib/signalr/signalr.js";
                yield return "https://ajax.aspnetcdn.com/ajax/globalize/0.1.1/globalize.js";
                yield return $"https://ajax.aspnetcdn.com/ajax/globalize/0.1.1/cultures/globalize.culture.{CultureInfo.CurrentCulture.Name}.js";
                yield return "/Areas/Admin/Content/lib/jquery/jquery-3.3.1.min.js";
                yield return "/Areas/Admin/Content/lib/jquery/jquery-ui-1.12.1/jquery-ui.js";
                yield return $"/Areas/Admin/Content/lib/jquery/ui/i18n/jquery.ui.datepicker-{CultureInfo.CurrentCulture.Name}.js";
                yield return "/Areas/Admin/Content/lib/jquery/jQuery-Timepicker-Addon-1.6.3/jquery-ui-timepicker-addon.js";
                yield return "/Areas/Admin/Content/lib/jquery.signalR-2.4.0.js";
                yield return "/Areas/Admin/Content/lib/store.js";
                yield return "/Areas/Admin/Content/lib/sweetalert-master/lib/sweet-alert.min.js";
                yield return "/Areas/Admin/Content/lib/jquery/validate/jquery.validate.js";
                yield return "/Areas/Admin/Content/lib/jquery/validate/jquery.validate.unobtrusive.js";
                yield return "/Areas/Admin/Content/lib/jquery/validate/unobtrusive-bootstrap.js";
                yield return "/Areas/Admin/Content/lib/bootstrap/dist/js/bootstrap.bundle.js";
                yield return "/Areas/Admin/Content/lib/bootstrap-confirmation.js";
                yield return "/Areas/Admin/Content/lib/adminlte/js/adminlte.js";
                yield return "/Areas/Admin/Content/lib/tag-it.min.js";
                yield return "/Areas/Admin/Content/lib/jquery.noty.packaged.js";
                yield return "/Areas/Admin/Content/lib/jstree/jstree.js";
                yield return "/Areas/Admin/Content/lib/dropzone.js";
                yield return "/Areas/Admin/Content/lib/featherlight.js";
                //yield return "https://cdnjs.cloudflare.com/ajax/libs/featherlight/1.7.13/featherlight.min.js";
                yield return "/Areas/Admin/Content/lib/jquery.are-you-sure.js";
                yield return "/Areas/Admin/Content/Lib/select2-4.0.13/js/select2.min.js";
                yield return "/Areas/Admin/Content/lib/spectrum/spectrum.js";
                yield return "/Areas/Admin/Content/lib/cropper/cropper.js";
                yield return "/Areas/Admin/Content/lib/cropper/jquery-cropper.js";
                yield return "https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.3/Chart.min.js";
            }
        }
    }
}
