using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldEditor
    {
        string View { get; }
        Dictionary<string, object> Config { get; }
        IEditorValueConverter ValueConverter { get; }

        string GetValue(IPublishedContent content, object value); //TODO: Check why we have this
    }
}
