using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsUmbraco14Migration : MigrationBase
    {
        public MetaFieldsUmbraco14Migration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            var itemsWithImages = Database.Fetch<MetaFieldsValueEntity>().Where(it => it.Alias == "openGraphImage");
            foreach (var item in itemsWithImages)
            {
                var actualValue = JsonConvert.DeserializeObject<string>(item.UserValue);
                if (!UdiParser.TryParse(actualValue, out var value))
                {
                    continue;
                }

                var guidUdi = new GuidUdi(value.UriValue);
                item.UserValue = JsonConvert.SerializeObject(guidUdi.Guid);

                Database.Update(item);
            }
        }
    }
}
