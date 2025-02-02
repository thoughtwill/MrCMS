using MrCMS.Entities;
using MrCMS.Entities.Multisite;

namespace MrCMS.AI.Entities.Settings
{
    /// <summary>
    ///     Represents a setting
    /// </summary>
    public class AiSetting : SystemEntity
    {
        /// <summary>
        ///     Gets or sets the type
        /// </summary>
        public virtual string SettingType { get; set; }

        /// <summary>
        ///     Gets or sets the property name
        /// </summary>
        public virtual string PropertyName { get; set; }

        /// <summary>
        ///     Gets or sets the value
        /// </summary>
        public virtual string Value { get; set; }
        public virtual Site Site { get; set; }
    }
}