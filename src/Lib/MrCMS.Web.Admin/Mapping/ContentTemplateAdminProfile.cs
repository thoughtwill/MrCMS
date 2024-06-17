using AutoMapper;
using MrCMS.ContentTemplates.Entities;
using MrCMS.ContentTemplates.Widgets;
using MrCMS.Web.Admin.Infrastructure.Mapping;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Widgets;

namespace MrCMS.Web.Admin.Mapping;

public class ContentTemplateAdminProfile : Profile
{
    public ContentTemplateAdminProfile()
    {
        CreateMap<ContentTemplate, AddContentTemplateModel>().ReverseMap();
        CreateMap<ContentTemplate, UpdateContentTemplateModel>().ReverseMap();
        
        CreateMap<ContentTemplateWidget, ContentTemplateWidgetAddModel>()
            .ForMember(f => f.ContentTemplateId, f => f.MapFrom(x => x.ContentTemplate.Id))
            .ReverseMap()
            .MapEntityLookup(f => f.ContentTemplateId, f => f.ContentTemplate);

        CreateMap<ContentTemplateWidget, ContentTemplateWidgetUpdateModel>()
            .ForMember(f => f.ContentTemplate, f => f.MapFrom(x => x.ContentTemplate))
            .ReverseMap()
            .ForMember(f => f.ContentTemplate, f => f.Ignore());
    }
}