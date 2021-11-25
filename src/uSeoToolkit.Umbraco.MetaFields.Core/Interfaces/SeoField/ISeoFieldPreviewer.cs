using System;
using System.Collections.Generic;
using System.Text;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldPreviewer
    {
        string Title { get; }
        string View { get; }
    }
}
