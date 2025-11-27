using System;
using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public interface ISeoSettingsRepository
    {
        bool IsEnabled(IContentType contentType);
        void Toggle(Guid contentTypeId, bool value);
    }
}
