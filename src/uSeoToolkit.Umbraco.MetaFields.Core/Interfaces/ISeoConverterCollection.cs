using System;
using System.Collections.Generic;
using System.Text;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces
{
    public interface ISeoConverterCollection
    {
        ISeoValueConverter GetConverter(Type fromType, Type toType);
    }
}
