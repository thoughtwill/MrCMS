using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using MrCMS.Entities;
using MrCMS.Helpers;

namespace MrCMS.DbConfiguration.Conventions
{
    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            // only apply this convention to entities that implement SystemEntity
            if (instance.EntityType.IsSubclassOf(typeof(SystemEntity)) &&
                instance.ChildType.IsSubclassOf(typeof(SystemEntity)))
            {
                instance.Cache.ReadWrite();
                instance.Cascade.SaveUpdate();
                instance.Cascade.AllDeleteOrphan();
                instance.Fetch.Subselect();
                instance.Inverse();
                instance.Relationship.NotFound.Ignore();
                instance.Key.ForeignKey(string.Format("FK_{0}_{1}", instance.ChildType.Name, instance.EntityType.Name));
                instance.Where("(IsDeleted = 'False' or IsDeleted = 0 or IsDeleted is null)");
            }
        }
    }
    //public class HasManyToManyConvention : IHasManyToManyConvention
    //{
    //    public void Apply(IManyToManyCollectionInstance instance)
    //    {
    //        instance.AsSet();
    //    }
    //}
}