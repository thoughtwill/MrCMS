using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.DbConfiguration.Overrides
{
    public class WebpageOverride : IAutoMappingOverride<Webpage>
    {
        public void Override(AutoMapping<Webpage> mapping)
        {
            mapping.DiscriminateSubClassesOnColumn("WebpageType");
            //mapping.Map(x => x.WebpageType, "WebpageType");


            mapping.Map(x => x.WebpageTypeName).Formula("WebpageType").Access.ReadOnly();

            mapping.HasManyToMany(webpage => webpage.Tags).Table("WebpageTags").Cascade.SaveUpdate();
            mapping.HasMany(webpage => webpage.Versions).KeyColumn("WebpageId").Cascade.All();
            //mapping.Map(x => x.WebpageType).Formula("WebpageType").Access.ReadOnly();
            mapping.HasManyToMany(x => x.TagPages).Table("WebpageTagPages").Cascade.SaveUpdate()
                .ChildWhere(x => x.IsDeleted == false);
            
            mapping.Map(webpage => webpage.BodyContent).MakeVarCharMax();
            mapping.Map(webpage => webpage.MetaTitle).MakeVarCharMax();
            mapping.Map(webpage => webpage.MetaKeywords).MakeVarCharMax();
            mapping.Map(webpage => webpage.MetaDescription).MakeVarCharMax();
            mapping.Map(webpage => webpage.Published).Not.Nullable().Default("0");
            mapping.Map(webpage => webpage.IncludeInSitemap).Not.Nullable().Default("1");

            mapping.Map(webpage => webpage.CustomFooterScripts).Length(8000);
            mapping.Map(webpage => webpage.CustomHeaderScripts).Length(8000);


            mapping.HasMany(webpage => webpage.Urls).Cascade.Delete();

            //Permission mappings
            mapping.HasManyToMany(webpage => webpage.FrontEndAllowedRoles)
                .Table("FrontEndWebpageRoles")
                .ParentKeyColumn("WebpageId");
        }
    }
}