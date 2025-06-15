using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class AddSeoToolkitSectionToAdminUserGroupMigration : AsyncMigrationBase
    {
        private readonly IUserGroupService _userGroupService;

        public AddSeoToolkitSectionToAdminUserGroupMigration(IMigrationContext context, IUserGroupService userGroupService)
           : base(context)
        {
            _userGroupService = userGroupService;
        }

        protected override async Task MigrateAsync()
        {
            var userGroup = (await _userGroupService.GetAllAsync(0, int.MaxValue)).Items.FirstOrDefault(it => it.Alias == UmbConstants.Security.AdminGroupAlias);

            if (userGroup != null && !userGroup.AllowedSections.Contains("SeoToolkit"))
            {
                userGroup.AddAllowedSection("SeoToolkit");

                await _userGroupService.UpdateAsync(userGroup, UmbConstants.Security.SuperUserKey);
            }
        }
    }
}
