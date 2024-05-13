using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Entities.BlockItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MrCMS.Web.Apps.Core.Entities.ContentBlocks;

public class Slider : IContentBlockWithSortableChildCollection
{
    public string DisplayName => $"Slider ({Slides.Count} slides)";
    public IReadOnlyList<BlockItem> Items => Slides;
    public List<Slide> Slides { get; set; } = new();

    public int Interval { get; set; } = 5000;

    public bool ShowIndicator { get; set; } = true;

    public bool PauseOnHover { get; set; }
    public string CaptionCssClass { get; set; } = "d-none d-md-block";

    public BlockItem AddChild()
    {
        var slide = new Slide();

        slide.Order = Slides.Count + 1;
        
        Slides.Add(slide);
        return slide;
    }

    public void Remove(BlockItem item)
    {
        Slides.Remove(item as Slide);
    }

    public void Sort(List<KeyValuePair<Guid, int>> orderedIds)
    {
        foreach (var slide in Slides)
        {
            slide.Order = orderedIds.FirstOrDefault(f => f.Key == slide.Id).Value;
        }
        Slides = Slides.OrderBy(f => f.Order).ToList();
    }
}