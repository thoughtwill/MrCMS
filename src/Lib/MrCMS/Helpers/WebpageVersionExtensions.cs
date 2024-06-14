using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MrCMS.Entities;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Models;
using Newtonsoft.Json;

namespace MrCMS.Helpers
{
    public static class WebpageVersionExtensions
    {
        public static T GetVersion<T>(this T doc, int id) where T : Webpage
        {
            var documentVersion = doc.Versions.FirstOrDefault(version => version.Id == id);
            if (documentVersion == null)
            {
                return null;
            }

            documentVersion = documentVersion.Unproxy();

            return documentVersion != null ? DeserializeVersion(documentVersion, doc) : null;
        }

        private static T DeserializeVersion<T>(WebpageVersion version, T doc) where T : Webpage
        {
            // use null handling ignore so that properties that didn't exist in previous versions are defaulted
            return JsonConvert.DeserializeObject(version.Data, doc.GetType(), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }) as T;
        }

        private static List<VersionChange> GetVersionChanges(Webpage currentVersion, Webpage previousVersion)
        {
            var changes = new List<VersionChange>();

            if (previousVersion == null)
                return changes;

            var propertyInfos = currentVersion.GetType().GetVersionProperties();

            changes.AddRange(from propertyInfo in propertyInfos
                             let oldValue = propertyInfo.GetValue(previousVersion, null)
                             let currentValue = propertyInfo.GetValue(currentVersion, null)
                             select new VersionChange
                             {
                                 Property =
                                                propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).
                                                    Any()
                                                    ? propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute),
                                                                                       true).OfType
                                                          <DisplayNameAttribute>().First().DisplayName
                                                    : propertyInfo.Name,
                                 PreviousValue = oldValue,
                                 CurrentValue = currentValue
                             });
            return changes;
        }

        public static bool AnyDifferencesFromCurrent(this WebpageVersion currentVersion)
        {
            return GetComparisonToCurrent(currentVersion).Any(change => change.AnyChange);
        }

        public static List<VersionChange> GetComparisonToCurrent(this WebpageVersion currentVersion)
        {
            var webpage = currentVersion.Webpage.Unproxy();
            WebpageVersion nextVersion = currentVersion.Webpage.Versions
                .Where(version => version.Id > currentVersion.Id).MinBy(version => version.Id);

            Webpage previousVersion = nextVersion != null
                ? DeserializeVersion(nextVersion, webpage)
                : webpage;

            return GetVersionChanges(previousVersion, DeserializeVersion(currentVersion, webpage));
        }

        public static List<PropertyInfo> GetVersionProperties(this Type type)
        {
            var ignorePropertyNames = new[]
                                          {
                                              "UpdatedOn", "Id", "CreatedOn"
                                          };

            return type.GetProperties().Where(
                info =>
                info.CanWrite &&
                !typeof(SystemEntity).IsAssignableFrom(info.PropertyType) &&
                (!info.PropertyType.IsGenericType ||
                 (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                &&
                !ignorePropertyNames.Contains(info.Name)).ToList();
        }
    }
}