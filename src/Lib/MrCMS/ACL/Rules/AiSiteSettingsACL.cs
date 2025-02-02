namespace MrCMS.ACL.Rules
{
    public class AiSiteSettingsACL : ACLRule
    {
        public const string View = "View";
        public const string Save = "Save";

        public override string DisplayName => "Ai Settings";
    }
}