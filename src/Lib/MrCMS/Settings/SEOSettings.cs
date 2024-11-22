using System.ComponentModel;

namespace MrCMS.Settings
{
    public class SEOSettings : SiteSettingsBase
    {
        [DisplayName("Robots.txt file")]
        [TextArea]
        public string RobotsText { get; set; }

        [DisplayName("Robots.txt file for staging")]
        [TextArea]
        public string RobotsTextStaging { get; set; }

        [DisplayName("Tracking Scripts (head)")]
        [TextArea]
        public string TrackingScripts { get; set; }

        [DisplayName("Tracking Scripts (top of body)")]
        [TextArea]
        public string TrackingScriptsBody { get; set; }
        
        [DisplayName("Enable Rich Snippets (JSON-LD)")]
        public bool EnableRichSnippets { get; set; } = true;

        public override bool RenderInSettings => true;
    }
}