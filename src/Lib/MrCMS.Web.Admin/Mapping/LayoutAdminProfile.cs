using AutoMapper;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Media;
using MrCMS.Web.Admin.Infrastructure.Mapping;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Mapping
{
    public class LayoutAdminProfile : Profile
    {
        public LayoutAdminProfile()
        {
            CreateMap<Layout, UpdateLayoutModel>()
                .ForMember(destinationMember => destinationMember.UrlSegment,
                    x => x.MapFrom(sourceMember => sourceMember.Path))
                .ReverseMap()
                .ForMember(destinationMember => destinationMember.Path,
                    x => x.MapFrom(sourceMember => sourceMember.UrlSegment));
            CreateMap<Layout, AddLayoutModel>()
                .ForMember(destinationMember => destinationMember.UrlSegment,
                    x => x.MapFrom(sourceMember => sourceMember.Path))
                .ReverseMap()
                .ForMember(destinationMember => destinationMember.Path,
                    x => x.MapFrom(sourceMember => sourceMember.UrlSegment))
                .MapEntityLookup(x => x.ParentId, x => x.Parent);
        }
    }

    public class MediaCategoryAdminProfile : Profile
    {
        public MediaCategoryAdminProfile()
        {
            CreateMap<MediaCategory, UpdateMediaCategoryModel>().ReverseMap();
            CreateMap<MediaCategory, AddMediaCategoryModel>()
                .ForMember(destinationMember => destinationMember.Path,
                    x => x.MapFrom(sourceMember => sourceMember.Path))
                .ReverseMap()
                .ForMember(destinationMember => destinationMember.Path,
                    x => x.MapFrom(sourceMember => sourceMember.Path))
                .MapEntityLookup(x => x.ParentId, x => x.Parent)
                ;
        }
    }
}