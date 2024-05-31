using System.Collections.Generic;
using MrCMS.Web.Admin.Models.RecurringJobs;

namespace MrCMS.Web.Admin.Services.RecurringJobs;

public interface IRecurringJobAdminService
{
    IReadOnlyList<RecurringJobInfo> GetRecurringJobs();
    SetupRecurringJobModel GetRecurringJobSetupModel(string id);
    
    void SetupRecurringJob(SetupRecurringJobModel model);
    
    void RemoveRecurringJob(string id);
    
    void TriggerRecurringJob(string id);
}