using System.Linq;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerSortOrderMigration : AsyncMigrationBase
    {
        public ScriptManagerSortOrderMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitScript", "SortOrder"))
            {
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitScript ADD SortOrder INT NOT NULL DEFAULT 0");

            var entries = Database.Fetch<ScriptEntity>(Sql().SelectAll().From<ScriptEntity>().OrderBy<ScriptEntity>(it => it.Id));
            var currentOrder = 1;
            foreach (var entry in entries)
            {
                Database.Execute("UPDATE SeoToolkitScript SET SortOrder = @0 WHERE Id = @1", currentOrder, entry.Id);
                currentOrder++;
            }

            return Task.CompletedTask;
        }
    }
}
