using System.Collections.Generic;
using MrCMS.Web.Admin.Models.Jobs;

namespace MrCMS.Web.Admin.Services.Jobs;

public interface IAdHocJobAdminService
{
    IReadOnlyList<AdHocJobInfo> GetAdHocJobs();
    AdHocJobInfo GetAdHocJob(string id);
    
    void TriggerAdHocJob(string id);
}