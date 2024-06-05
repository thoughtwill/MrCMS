using System;
using JetBrains.Annotations;

namespace MrCMS.Web.Admin.Models.RecurringJobs;

public class RecurringJobInfo
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; }
    [CanBeNull] public string Cron { get; set; }
    public DateTimeOffset? LastExecution { get; set; }
    public DateTimeOffset? NextExecution { get; set; }
    
    public bool IsManaged { get; set; }
}