using System.Collections.Generic;
using MrCMS.Web.Admin.Models.Jobs;

namespace MrCMS.Web.Admin.Services.Jobs;

public interface IRecurringJobAdminService
{
    IReadOnlyList<RecurringJobInfo> GetRecurringJobs();
    SetupRecurringJobModel GetRecurringJobSetupModel(string id);
    
    void SetupRecurringJob(SetupRecurringJobModel model);
    
    void RemoveRecurringJob(string id);
    
    void TriggerRecurringJob(string id);
}