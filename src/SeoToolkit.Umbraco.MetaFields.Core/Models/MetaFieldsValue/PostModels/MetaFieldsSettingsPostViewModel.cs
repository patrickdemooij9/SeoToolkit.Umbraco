using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.PostModels
{
    public class MetaFieldsSettingsPostViewModel
    {
        public int NodeId { get; set; }
        public int ContentTypeId { get; set; }
        public string Culture { get; set; }
        public Dictionary<string, object> UserValues { get; set; }
    }
}
