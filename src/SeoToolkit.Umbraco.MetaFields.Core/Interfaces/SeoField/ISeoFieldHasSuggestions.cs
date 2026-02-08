using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldHasSuggestions
    {
        List<ISeoFieldSuggestion> Suggestions { get; }
    }
}