using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business
{
    public class DocumentTypeSettingsDto
    {
        public IContentType Content { get; set; }
        public bool EnableSeoSettings { get; set; }
        public Dictionary<ISeoField, DocumentTypeValueDto> Fields { get; set; }
        public IContentType Inheritance { get; set; }

        public DocumentTypeSettingsDto()
        {
            Fields = new Dictionary<ISeoField, DocumentTypeValueDto>();
        }

        public DocumentTypeValueDto Get(string alias)
        {
            var valuePair = Fields.FirstOrDefault(it => it.Key.Alias.Equals(alias));
            return valuePair.Key is null ? null : valuePair.Value;
        }
    }
}
