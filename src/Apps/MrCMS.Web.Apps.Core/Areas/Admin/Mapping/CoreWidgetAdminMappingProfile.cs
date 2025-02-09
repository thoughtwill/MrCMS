using AutoMapper;
using MrCMS.Web.Apps.Core.Areas.Admin.Models.Widgets;
using MrCMS.Web.Apps.Core.Widgets;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Mapping
{
    public class CoreWidgetAdminMappingProfile : Profile
    {
        public CoreWidgetAdminMappingProfile()
        {
            CreateMap<CurrentPageSubNavigation, CurrentPageSubNavigationModel>().ReverseMap();
            CreateMap<LinkedImage, LinkedImageModel>().ReverseMap();
            CreateMap<Navigation, NavigationModel>().ReverseMap();
            CreateMap<CodeWidget, CodeWidgetModel>().ReverseMap();
            CreateMap<TextWidget, TextWidgetModel>().ReverseMap();
            CreateMap<SliderWidget, SliderWidgetModel>()
                .ForMember(x => x.SlideList, x => x.MapFrom(f => f.Slides))
                .ReverseMap()
                .ForMember(x => x.SlideList, x => x.MapFrom(f => f.ToString()));
        }
    }
}