using Microsoft.Extensions.Configuration;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.NotFound.Core.Components;
using SeoToolkit.Umbraco.NotFound.Core.Config;
using SeoToolkit.Umbraco.NotFound.Core.ContentFinders;
using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.NotFound.Core.Composers;

public class NotFoundComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var section = builder.Config.GetSection("SeoToolkit:NotFound");
        var settings = section?.Get<NotFoundAppSettingsModel>();
        var disabledModules = settings?.DisabledModules ?? Array.Empty<string>();

        if (disabledModules.Contains(DisabledModuleConstant.All))
        {
            builder.Components().Append<DisableModuleComponent>();
            return;
        }

        if (!disabledModules.Contains(DisabledModuleConstant.LastChanceContentFinder))
        {
            builder.SetContentLastChanceFinder<PageNotFoundFinder>();
        }

        builder.Components().Append<EnableModuleComponent>();
    }
}