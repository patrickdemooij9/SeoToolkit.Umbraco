using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.PostModels
{
    public class SeoSettingsPostViewModel
    {
        public int NodeId { get; set; }
        public int ContentTypeId { get; set; }
        public string Culture { get; set; }
        public Dictionary<string, object> UserValues { get; set; }
    }
}
