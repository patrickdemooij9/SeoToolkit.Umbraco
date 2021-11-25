using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoConverterCollection : BuilderCollectionBase<ISeoValueConverter>, ISeoConverterCollection
    {
        public SeoConverterCollection(IEnumerable<ISeoValueConverter> items) : base(() => items)
        {
        }

        public ISeoValueConverter GetConverter(Type fromType, Type toType)
        {
            var exactConverter = this.FirstOrDefault(it => it.FromValue == fromType && it.ToValue == toType);
            if (exactConverter != null)
                return exactConverter;
            return this.FirstOrDefault(it => it.FromValue.IsAssignableFrom(fromType) && it.ToValue == toType);
        }
    }
}
