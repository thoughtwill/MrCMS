using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Helpers
{
    public static class SelectListItemHelper
    {
        public static List<SelectListItem> BuildSelectItemList<T>(this IEnumerable<T> items, Func<T, string> text,
            Func<T, string> value = null, Func<T, bool> selected = null, string emptyItemText = null,
            string group = null)
        {
            return BuildSelectItemList(items, text, value, selected,
                !string.IsNullOrWhiteSpace(emptyItemText) ? EmptyItem(emptyItemText) : null,
                !string.IsNullOrWhiteSpace(group) ? new SelectListGroup { Name = group } : null);
        }

        public static List<SelectListItem> BuildSelectItemList<T>(this IEnumerable<T> items, Func<T, string> text,
            Func<T, string> value = null, Func<T, bool> selected = null, SelectListItem emptyItem = null,
            SelectListGroup group = null)
        {
            var selectListItems =
                items.Select(x =>
                    new SelectListItem
                    {
                        Text = text.Invoke(x),
                        Value = value == null ? text.Invoke(x) : value.Invoke(x),
                        Selected = selected != null && selected.Invoke(x),
                        Group = group
                    });

            var listItems = selectListItems as SelectListItem[] ?? selectListItems.ToArray();
            if (emptyItem != null)
            {
                emptyItem.Selected = !listItems.Any(f => f.Selected);
            }

            return (emptyItem != null
                ? new List<SelectListItem> { emptyItem }.Union(listItems)
                : listItems).ToList();
        }

        public static async Task<List<SelectListItem>> BuildSelectItemListAsync<T>(
            this IEnumerable<T> items,
            Func<T, Task<string>> text,
            Func<T, Task<string>> value = null,
            Func<T, Task<bool>> selected = null,
            string emptyItemText = null,
            string group = null
        )
        {
            return await BuildSelectItemListAsync(items, text, value, selected,
                !string.IsNullOrWhiteSpace(emptyItemText) ? EmptyItem(emptyItemText) : null, !string.IsNullOrWhiteSpace(group) ? new SelectListGroup { Name = group } : null);
        }

        public static async Task<List<SelectListItem>> BuildSelectItemListAsync<T>(
            this IEnumerable<T> items,
            Func<T, Task<string>> text,
            Func<T, Task<string>> value = null,
            Func<T, Task<bool>> selected = null,
            SelectListItem emptyItem = null,
            SelectListGroup group = null
        )
        {
            var selectListItems = new List<SelectListItem>();
            foreach (var item in items)
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = await text.Invoke(item),
                    Value = value == null ? await text.Invoke(item) : await value.Invoke(item),
                    Selected = selected != null && await selected.Invoke(item),
                    Group = group
                });
            }

            if (emptyItem != null)
            {
                emptyItem.Selected = !selectListItems.Any(f => f.Selected);
            }

            return (emptyItem != null
                ? new List<SelectListItem> { emptyItem }.Union(selectListItems)
                : selectListItems).ToList();
        }

        public static SelectListItem EmptyItem(string text = null, string value = "")
        {
            return new() { Text = text ?? "Please select...", Value = value, Selected = true };
        }
    }
}