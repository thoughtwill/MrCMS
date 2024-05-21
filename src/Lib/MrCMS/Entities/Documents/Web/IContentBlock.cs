using System.Collections.Generic;

namespace MrCMS.Entities.Documents.Web;

public interface IContentBlock
{
    string DisplayName { get; }
    IReadOnlyList<BlockItem> Items { get; }
}