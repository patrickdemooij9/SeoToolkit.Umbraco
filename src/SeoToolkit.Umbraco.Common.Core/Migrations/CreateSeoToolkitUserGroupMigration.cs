using System.Linq;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class CreateSeoToolkitUserGroupMigration : MigrationBase
    {
        private const string UserGroupAlias = "SeoToolkit";

        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUserService _userService;

        public CreateSeoToolkitUserGroupMigration(
            IMigrationContext context,
            IShortStringHelper shortStringHelper,
            IUserService userService)
           : base(context)
        {
            _shortStringHelper = shortStringHelper;
            _userService = userService;
        }

        protected override void Migrate()
        {
            var userGroups = _userService.GetAllUserGroups().ToArray();
            if (userGroups.Any(it => it.Alias == UserGroupAlias || it.Name == "SEO Toolkit"))
                return;

            var userGroup = new UserGroup(_shortStringHelper)
            {
                Alias = UserGroupAlias,
                Name = "SEO Toolkit",
                Icon = "icon-globe-alt",
            };

            userGroup.AddAllowedSection("SeoToolkit");

            _userService.Save(userGroup);
        }
    }
}
