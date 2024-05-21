using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace MrCMS.Settings
{
    public class SiteSettings : SiteSettingsBase
    {
        public SiteSettings()
        {
            DefaultPageSize = 10;
            Log404s = true;
            CKEditorConfig = SettingDefaults.CkEditorConfig;
            HoneypotFieldName = "YourStatus";
            DaysToKeepLogs = 30;
            TaskExecutorKey = "executor";
            TaskExecutorPassword = Guid.NewGuid().ToString();
            TaskExecutionDelay = 10;
        }


        [DropDownSelection("Themes"), DisplayName("Theme")]
        public string ThemeName { get; set; }

        [DisplayName("Default Layout"), DropDownSelection("DefaultLayoutOptions")]
        public int DefaultLayoutId { get; set; }

        [DisplayName("User Account Layout"), DropDownSelection("DefaultUserAccountLayoutOptions")]
        public int UserAccountLayoutId { get; set; }

        [DisplayName("Default Page Size")] public int DefaultPageSize { get; set; }

        [DisplayName("Home Page"), DropDownSelection("HomePageOptions")]
        public int HomePageId { get; set; }

        [DisplayName("Enable inline editing")] public bool EnableInlineEditing { get; set; }

        [DisplayName("Log 404 in admin logs")] public bool Log404s { get; set; }


        [DisplayName("Site UI Culture"), DropDownSelection("UiCultures")]
        public string UICulture { get; set; }

        [MediaSelector] public string AdminLogo { get; set; }

        public CultureInfo CultureInfo =>
            !String.IsNullOrWhiteSpace(UICulture)
                ? CultureInfo.GetCultureInfo(UICulture)
                : CultureInfo.CurrentCulture;

        [DisplayName("Honeypot Field Name")] public string HoneypotFieldName { get; set; }

        public string TaskExecutorKey { get; set; }
        public string TaskExecutorPassword { get; set; }
        public int TaskExecutionDelay { get; set; }

        public bool HasHoneyPot => !string.IsNullOrWhiteSpace(HoneypotFieldName);

        [DisplayName("CKEditor Config"), TextArea]
        public string CKEditorConfig { get; set; }

        public int DaysToKeepLogs { get; set; }

        [DisplayName("Allowed Admin IPs")] public string AllowedAdminIPs { get; set; }

        public IEnumerable<string> AllowedIPs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AllowedAdminIPs)) yield break;
                string[] ips = AllowedAdminIPs.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ip in ips)
                {
                    IPAddress address;
                    if (IPAddress.TryParse(ip, out address))
                        yield return ip;
                }
            }
        }

        public override bool RenderInSettings => true;

        [DisplayName("Default Form Renderer Type"), DropDownSelection("DefaultFormRenderer")]
        public FormRenderingType FormRendererType { get; set; }
        
        [DisplayName("Default Form Label Render Type"), DropDownSelection("DefaultFormLabelRenderer")]
        public FormLabelRenderingType FormLabelRenderingType { get; set; }

        [TextArea] public string GDPRFairProcessingText { get; set; }


        public override void SetViewData(IServiceProvider serviceProvider, ViewDataDictionary viewDataDictionary)
        {
            var generator = serviceProvider.GetRequiredService<ISiteSettingsOptionGenerator>();
            viewDataDictionary["DefaultLayoutOptions"] = generator.GetLayoutOptions(DefaultLayoutId);
            viewDataDictionary["DefaultUserAccountLayoutOptions"] = generator.GetLayoutOptions(UserAccountLayoutId);
            viewDataDictionary["Themes"] = generator.GetThemeNames(ThemeName);

            viewDataDictionary["UiCultures"] = generator.GetUiCultures(UICulture);
            viewDataDictionary["HomePageOptions"] = generator.GetTopLevelPageOptions(HomePageId);
            
            viewDataDictionary["DefaultFormRenderer"] =
                generator.GetFormRendererOptions(FormRendererType);
            
            viewDataDictionary["DefaultFormLabelRenderer"] =
                generator.GetFormLabelRendererOptions(FormLabelRenderingType);
        }

        public TagBuilder GetHoneypot()
        {
            var honeyPot = new TagBuilder("input");
            honeyPot.Attributes["type"] = "text";
            honeyPot.Attributes["style"] = "display:none; visibility: hidden;";
            honeyPot.Attributes["name"] = HoneypotFieldName;
            honeyPot.TagRenderMode = TagRenderMode.SelfClosing;
            return honeyPot;
        }
    }
}