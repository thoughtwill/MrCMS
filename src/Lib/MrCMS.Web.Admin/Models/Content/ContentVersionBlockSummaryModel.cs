using System;
using System.Collections.Generic;
using System.Linq;

namespace MrCMS.Web.Admin.Models.Content;

public class ContentVersionBlockSummaryModel
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string DisplayName { get; set; }
    public bool IsHidden { get; set; }
    public Guid Guid { get; set; }
    public Type Type { get; set; }
    public List<ContentVersionBlockItemSummaryModel> Items { get; set; }
    public bool ShowCaret => Items.Any() || CanAddChildren;
    public bool CanAddChildren { get; set; }
    public bool CanOrderChildren { get; set; }
}