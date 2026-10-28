using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.PostModels;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitDashboard")]
    public class DashboardController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly DashboardDataSourceCollection _dataSources;

        public DashboardController(DashboardDataSourceCollection dataSources)
        {
            _dataSources = dataSources;
        }

        [HttpPost("data")]
        [ProducesResponseType(typeof(DashboardData), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetData(DashboardDataPostModel postModel)
        {
            var dataSource = _dataSources.Get(postModel.DataSource);
            if (dataSource is null)
                return NotFound();

            return Ok(await dataSource.GetAsync(postModel.Parameters ?? new Dictionary<string, string>()));
        }
    }
}
