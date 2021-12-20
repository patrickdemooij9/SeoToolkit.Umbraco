using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class UmbracoMediaUdiConverter : UmbracoUdiConverter
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public UmbracoMediaUdiConverter(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override object ConvertDatabaseToObject(object value)
        {
            if (!UdiParser.TryParse(value?.ToString(), out var udi))
                return null;

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                return ctx.UmbracoContext.Media.GetById(udi);
            }
        }
    }
}
