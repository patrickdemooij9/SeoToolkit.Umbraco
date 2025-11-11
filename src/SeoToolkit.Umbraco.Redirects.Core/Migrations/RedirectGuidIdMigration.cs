using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectGuidIdMigration : AsyncMigrationBase
    {
        public RedirectGuidIdMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitRedirects", "Key"))
            {
                return Task.CompletedTask;
            }

            //TODO: Implement SQLLite compatibility
            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD COLUMN [Key] UNIQUEIDENTIFIER NULL");
            Database.Execute("UPDATE SeoToolkitRedirects SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitRedirects DROP CONSTRAINT pk_SeoToolkitRedirects");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CONSTRAINT pk_SeoToolkitRedirects PRIMARY KEY ([Key])");
            return Task.CompletedTask;
        }
    }
}
