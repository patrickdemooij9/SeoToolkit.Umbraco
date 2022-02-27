using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.Common.Core.Sections;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace uSeoToolkit.Umbraco.Common.Core.Migrations
{
    public class AddSeoToolkitSectionToAdminUserGroupMigration : MigrationBase
    {
        private readonly IUserService _userService;

        public AddSeoToolkitSectionToAdminUserGroupMigration(IMigrationContext context, IUserService userService)
           : base(context)
        {
            _userService = userService;
        }

        protected override void Migrate()
        {
            var userGroup = _userService.GetUserGroupByAlias(UmbConstants.Security.AdminGroupAlias);

            if (userGroup != null)
            {
                userGroup.AddAllowedSection(USeoToolkitSection.SectionAlias);

                _userService.Save(userGroup);
            }
        }
    }
}
