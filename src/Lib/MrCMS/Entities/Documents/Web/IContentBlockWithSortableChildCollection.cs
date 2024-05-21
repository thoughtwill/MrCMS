using System;
using System.Collections.Generic;

namespace MrCMS.Entities.Documents.Web;

public interface IContentBlockWithSortableChildCollection : IContentBlockWithChildCollection
{
    void Sort(List<KeyValuePair<Guid, int>> orderedIds);
}