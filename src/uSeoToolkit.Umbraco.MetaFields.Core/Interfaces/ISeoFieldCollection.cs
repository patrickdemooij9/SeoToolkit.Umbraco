using System;
using System.Collections.Generic;
using System.Text;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces
{
    public interface ISeoFieldCollection
    {
        ISeoField Get(string alias);
        IEnumerable<ISeoField> GetAll();
    }
}
