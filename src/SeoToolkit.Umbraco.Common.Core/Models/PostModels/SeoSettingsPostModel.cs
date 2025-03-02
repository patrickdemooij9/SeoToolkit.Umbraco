using System;

namespace SeoToolkit.Umbraco.Common.Core.Models.PostModels
{
    public class SeoSettingsPostModel
    {
        public Guid ContentTypeId { get; set; }
        public bool Enabled { get; set; }
    }
}
