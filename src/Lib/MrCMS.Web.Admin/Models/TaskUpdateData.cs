using System;
using MrCMS.Helpers;

namespace MrCMS.Web.Admin.Models
{
    public class TaskUpdateData
    {
        public string Name { get; set; }
        public string TypeName { get; set; }

        public Type Type => TypeHelper.GetTypeByName(TypeName);

        public bool Enabled { get; set; }
        public string CronSchedule { get; set; }
    }
}