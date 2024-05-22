using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Tasks
{
    public class ClearFormEntries : IClearFormEntries
    {
        private readonly ISession _session;

        public ClearFormEntries(ISession session)
        {
            _session = session;
        }
        
        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task Execute()
        {
            var formsToCheck = _session.Query<Form>().Where(x => x.DeleteEntriesAfter != null).ToList();

            var toDelete = new List<FormPosting>();

            foreach (var form in formsToCheck)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-form.DeleteEntriesAfter.GetValueOrDefault());
                toDelete.AddRange(_session.Query<FormPosting>()
                    .Where(x => x.Form.Id == form.Id && x.CreatedOn < cutoffDate));
            }

            if (toDelete.Any())
            {
                await _session.TransactAsync(async session =>
                {

                    foreach (var formPosting in toDelete)
                    {
                        await session.DeleteAsync(formPosting);
                    }
                });
            }
        }
    }

    public interface IClearFormEntries
    {
        Task Execute();
    }
}