using System.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Common.Core.Sections;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
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

            if (userGroup != null && !userGroup.AllowedSections.Contains(SeoToolkitSection.SectionAlias))
            {
                userGroup.AddAllowedSection(SeoToolkitSection.SectionAlias);

                _userService.Save(userGroup);
            }
        }
    }
}
