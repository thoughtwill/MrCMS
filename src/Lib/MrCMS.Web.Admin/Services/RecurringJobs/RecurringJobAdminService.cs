using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using Hangfire;
using Hangfire.Storage;
using MrCMS.Helpers;
using MrCMS.Tasks;
using MrCMS.Web.Admin.Models.RecurringJobs;

namespace MrCMS.Web.Admin.Services.RecurringJobs;

public class RecurringJobAdminService : IRecurringJobAdminService
{
    private const string RecurringJobLastExecutionsKey = "recurring-job:last-executions";
    private static readonly IReadOnlyDictionary<string, MrCMSRecurringJob> MrCMSRecurringJobTypes = Initialize();

    private static IReadOnlyDictionary<string, MrCMSRecurringJob> Initialize()
    {
        var types = TypeHelper.GetAllConcreteTypesAssignableFrom<MrCMSRecurringJob>();

        var jobs = types.Where(x => x.HasParameterlessConstructor())
            .Select(x => (MrCMSRecurringJob)Activator.CreateInstance(x))
            .ToHashSet();

        return jobs.ToDictionary(x => x.Id, x => x);
    }

    public IReadOnlyList<RecurringJobInfo> GetRecurringJobs()
    {
        // start by getting all recurring jobs active in hangfire
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();
        // todo - possibly retrieve historical data for LastExecution and NextExecution from Hangfire sets
        var setValues = connection.GetAllItemsFromSet(RecurringJobLastExecutionsKey);
        var lastExecutions = setValues.Select(x =>
        {
            var parts = x.Split('|');
            return new
            {
                Id = parts[0],
                LastExecution = DateTimeOffset.Parse(parts[1]).ToLocalTime()
            };
        }).ToDictionary(x => x.Id, x => x.LastExecution);

        var result = new List<RecurringJobInfo>();


        // first add records for all the jobs in Hangfire
        foreach (var jobDto in jobs)
        {
            var matchingJob = MrCMSRecurringJobTypes.GetValueOrDefault(jobDto.Id);
            result.Add(new RecurringJobInfo
            {
                Id = jobDto.Id,
                IsActive = true,
                Cron = jobDto.Cron,
                LastExecution = ToDateTimeOffset(jobDto.LastExecution) ??
                                (lastExecutions.TryGetValue(jobDto.Id, out var execution) ? execution : null),
                NextExecution = ToDateTimeOffset(jobDto.NextExecution),
                IsManaged = matchingJob != null
            });
        }

        // then for any MrCMSRecurringJob that is not in Hangfire, add a record
        foreach (var job in MrCMSRecurringJobTypes.Values)
        {
            if (result.All(x => x.Id != job.Id))
            {
                result.Add(new RecurringJobInfo
                {
                    Id = job.Id,
                    IsActive = false,
                    Cron = null,
                    IsManaged = true,
                    LastExecution = // get the last execution from the set of last executions
                        lastExecutions.TryGetValue(job.Id, out var execution) ? execution : null
                });
            }
        }

        // sort the list by Id
        result = result.OrderBy(x => x.Id).ToList();

        return result;
    }

    public SetupRecurringJobModel GetRecurringJobSetupModel(string id)
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();

        return new SetupRecurringJobModel
        {
            Id = id,
            Cron = jobs.FirstOrDefault(x => x.Id == id)?.Cron,
        };
    }

    public void SetupRecurringJob(SetupRecurringJobModel model)
    {
        var job = MrCMSRecurringJobTypes.GetValueOrDefault(model.Id);

        job?.OnAddOrUpdate(model.Cron);
    }


    public void RemoveRecurringJob(string id)
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();
        // find the existing job
        var job = jobs.FirstOrDefault(x => x.Id == id);
        if (job == null)
            return;

        // find the last execution
        var jobLastExecution = job.LastExecution;
        if (jobLastExecution != null)
        {
            // add the last execution to the set of last executions
            using var transaction = connection.CreateWriteTransaction();
            transaction.AddToSet(RecurringJobLastExecutionsKey,
                id + "|" + jobLastExecution.Value.ToString("O"));

            transaction.Commit();
        }

        RecurringJob.RemoveIfExists(id);
    }

    public void TriggerRecurringJob(string id)
    {
        RecurringJob.TriggerJob(id);
    }


    private DateTimeOffset? ToDateTimeOffset(DateTime? utcDateTime)
    {
        if (utcDateTime == null)
            return null;

        // Step 2: Get the local time zone
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

        // Step 3: Convert the UTC DateTime to the local time zone
        DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, localTimeZone);

        // Step 4: Create a DateTimeOffset from the local DateTime
        return new DateTimeOffset(localDateTime, localTimeZone.GetUtcOffset(localDateTime));
    }
}