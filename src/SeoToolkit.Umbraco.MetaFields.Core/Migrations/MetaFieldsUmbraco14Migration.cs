using Newtonsoft.Json;
using NPoco;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsUmbraco14Migration : AsyncMigrationBase
    {
        public MetaFieldsUmbraco14Migration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            var itemsWithImages = Database.Fetch<OldMetaFieldsValueEntity>().Where(it => it.Alias == "openGraphImage");
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
            return Task.CompletedTask;
        }

        [TableName("SeoToolkitMetaFieldsValue")]
        [ExplicitColumns]
        [PrimaryKey(new[] { "NodeId", "Alias", "Culture" })]
        private class OldMetaFieldsValueEntity
        {
            [Column("NodeId")]
            [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "NodeId, Alias, Culture")]
            public int NodeId { get; set; }

            [Column("Alias")]
            public string Alias { get; set; }

            [Column("Culture")]
            public string Culture { get; set; } = "";

            [Column("UserValue")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string UserValue { get; set; }
        }
    }
}
