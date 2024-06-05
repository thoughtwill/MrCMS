using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using Hangfire;
using Hangfire.Storage;
using MrCMS.Helpers;
using MrCMS.Jobs;
using MrCMS.Web.Admin.Models.Jobs;

namespace MrCMS.Web.Admin.Services.Jobs;

public class AdHocJobAdminService : IAdHocJobAdminService
{
    private static readonly IReadOnlyDictionary<string, MrCMSAdHocJob> MrCMSAdHocJobTypes = Initialize();

    private static Dictionary<string, MrCMSAdHocJob> Initialize()
    {
        var types = TypeHelper.GetAllConcreteTypesAssignableFrom<MrCMSAdHocJob>();

        var jobs = types.Where(x => x.HasParameterlessConstructor())
            .Select(x => (MrCMSAdHocJob)Activator.CreateInstance(x))
            .ToHashSet();

        return jobs.ToDictionary(x => x.Id, x => x);
    }

    public IReadOnlyList<AdHocJobInfo> GetAdHocJobs()
    {
        using var connection = JobStorage.Current.GetConnection();

        return MrCMSAdHocJobTypes.Select(job => new AdHocJobInfo
        {
            Id = job.Key,
            DisplayName = job.Value.DisplayName,
            LastExecution = connection.GetLastExecution(job.Value)
        }).ToList();
    }

    public AdHocJobInfo GetAdHocJob(string id)
    {
        var job = MrCMSAdHocJobTypes.GetValueOrDefault(id);
        if (job == null)
            throw new ArgumentException("Job not found", nameof(id));

        using var connection = JobStorage.Current.GetConnection();

        return new AdHocJobInfo
        {
            Id = job.Id,
            DisplayName = job.DisplayName,
            LastExecution = connection.GetLastExecution(job)
        };
    }


    public void TriggerAdHocJob(string id)
    {
        var job = MrCMSAdHocJobTypes.GetValueOrDefault(id);
        if (job == null)
            throw new ArgumentException("Job not found", nameof(id));

        var jobId = job.QueueJob();

        using var connection = JobStorage.Current.GetConnection();

        connection.AddJobMetadata(jobId, "job-type", job.Id);
        connection.AddJobMetadata(jobId, "timestamp", DateTimeOffset.UtcNow.ToString("O"));
        var key = GetTriggeredKey(job);

        // Store in a dedicated set for quick access
        using var transaction = connection.CreateWriteTransaction();

        transaction.AddToSet(key, jobId);
        transaction.Commit();
    }

    public static string GetTriggeredKey(MrCMSAdHocJob job)
    {
        return "job-type:triggered:" + job.Id;
    }
}

public static class AdHocJobAdminServiceExtensions
{
    public static void AddJobMetadata(this IStorageConnection connection, string jobId, string key, string value)
    {
        connection.SetRangeInHash($"job:{jobId}:metadata", new[] { new KeyValuePair<string, string>(key, value) });
    }

    public static IDictionary<string, string> GetJobMetadata(this IStorageConnection connection, string jobId)
    {
        return connection.GetAllEntriesFromHash($"job:{jobId}:metadata");
    }

    public static DateTimeOffset? GetLastExecution(this IStorageConnection connection, MrCMSAdHocJob job)
    {
        var jobIds = connection.GetAllItemsFromSet(AdHocJobAdminService.GetTriggeredKey(job));
        if (jobIds.Count == 0)
            return null;
        DateTime? maxTimestamp = null;

        foreach (var jobId in jobIds)
        {
            var stateData = connection.GetStateData(jobId);

            if (stateData is not { Name: "Succeeded" })
            {
                continue;
            }

            if (stateData.Data.TryGetValue("SucceededAt", out var succeededAt) &&
                DateTime.TryParse(succeededAt, out var succeededAtDate) &&
                (maxTimestamp == null || succeededAtDate > maxTimestamp))
            {
                maxTimestamp = new DateTime(succeededAtDate.Ticks, DateTimeKind.Utc);
            }
        }

        // convert to local time
        return maxTimestamp;
    }
}