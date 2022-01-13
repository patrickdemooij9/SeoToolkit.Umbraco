using System.Linq;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Services
{
    public class RobotsTxtService : IRobotsTxtService
    {
        private IRobotsTxtRepository _robotsTxtRepository;

        public RobotsTxtService(IRobotsTxtRepository robotsTxtRepository)
        {
            _robotsTxtRepository = robotsTxtRepository;
        }

        public string GetContent()
        {
            //This will probably support multiple domains later on, but for now we can just take the only one
            return _robotsTxtRepository.GetAll().FirstOrDefault()?.Content ?? string.Empty;
        }

        public void SetContent(string content)
        {
            var model = _robotsTxtRepository.GetAll().FirstOrDefault();
            if (model is null)
                model = new RobotsTxtModel();

            model.Content = content;
            _robotsTxtRepository.Update(model);
        }
    }
}
