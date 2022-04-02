using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldRenderer
    {
        string GetValue(string value);
    }
}
