using System;

namespace MrCMS.Web.Admin.Models.Jobs;

public class AdHocJobInfo
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public DateTimeOffset? LastExecution { get; set; }
}