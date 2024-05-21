namespace MrCMS.ACL.Rules
{
    public class CoreACL : ACLRule
    {
        public const string ManagePages = "Manage Pages";
        public const string CanViewUnpublishedPages = "Can View Unpublished Pages";
        public const string ManageContentEditor = "Manage Content Editor";
        public const string ManageMedia = "Manage Media";
        public const string ManageLayouts = "Manage Layouts";
        public const string ManageForms = "Manage Forms";

        public override string DisplayName => "Core Functionality";

        //protected override List<string> GetOperations()
        //{
        //    return new List<string>
        //    {
        //        ManagePages,
        //        ManageMedia,
        //        ManageLayouts,
        //    };
        //}
    }
}