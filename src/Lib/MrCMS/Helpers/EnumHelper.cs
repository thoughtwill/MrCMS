using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Helpers
{
    public static class EnumHelper<T>
    {
        public static IList<T> GetValues()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames()
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

        public static IList<string> GetDisplayValues()
        {
            return GetValues().Select(GetDisplayValue).ToList();
        }

        public static string GetDisplayValue(T value)
        {
            var fieldInfo = typeof(T).GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            var defaultValue = value.ToString().BreakUpString();

            if (descriptionAttributes == null)
                return defaultValue;

            return descriptionAttributes.Length > 0
                ? descriptionAttributes[0].Name
                : defaultValue;
        }

        public static List<SelectListItem> GetOptions(SelectListItem emptyItem = null)
        {
            return GetValues()
                .BuildSelectItemList(GetDisplayValue, arg => arg.ToString(), emptyItem: emptyItem);
        }
    }
}