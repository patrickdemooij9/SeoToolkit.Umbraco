using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Services
{
    public class RobotsTxtService : IRobotsTxtService
    {
        private IRobotsTxtRepository _robotsTxtRepository;
        private readonly IRobotsTxtValidator _robotsTxtValidator;

        public RobotsTxtService(IRobotsTxtRepository robotsTxtRepository,
            IRobotsTxtValidator robotsTxtValidator)
        {
            _robotsTxtRepository = robotsTxtRepository;
            _robotsTxtValidator = robotsTxtValidator;
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

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            return _robotsTxtValidator.Validate(content);
        }
    }
}
