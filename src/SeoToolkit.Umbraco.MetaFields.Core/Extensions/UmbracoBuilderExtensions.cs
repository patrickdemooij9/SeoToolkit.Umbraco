using System;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.MetaFields.Core.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder UpdateMetaField<TField>(this IUmbracoBuilder builder, Action<TField> update)
            where TField : ISeoField
        {
            builder.WithCollectionBuilder<SeoFieldCollectionBuilder>().Update(update);
            return builder;
        }
    }
}
