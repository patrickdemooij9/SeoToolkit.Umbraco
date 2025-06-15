using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class CreateSeoToolkitUserGroupMigration : AsyncMigrationBase
    {
        private const string UserGroupAlias = "SeoToolkit";

        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUserGroupService _userGroupService;

        public CreateSeoToolkitUserGroupMigration(
            IMigrationContext context,
            IShortStringHelper shortStringHelper,
            IUserGroupService userGroupService)
           : base(context)
        {
            _shortStringHelper = shortStringHelper;
            _userGroupService = userGroupService;
        }

        protected override async Task MigrateAsync()
        {
            var userGroups = (await _userGroupService.GetAllAsync(0, int.MaxValue)).Items.ToArray();
            if (userGroups.Any(it => it.Alias == UserGroupAlias || it.Name == "SEO Toolkit"))
                return;

            var userGroup = new UserGroup(_shortStringHelper)
            {
                Alias = UserGroupAlias,
                Name = "SEO Toolkit",
                Icon = "icon-globe-alt",
            };

            userGroup.AddAllowedSection("SeoToolkit");

            await _userGroupService.CreateAsync(userGroup, UmbConstants.Security.SuperUserKey);
        }
    }
}
