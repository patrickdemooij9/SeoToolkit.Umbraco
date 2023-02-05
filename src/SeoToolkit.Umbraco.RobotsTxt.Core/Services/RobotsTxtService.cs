using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Services
{
    public class RobotsTxtService : IRobotsTxtService
    {
        private readonly IRobotsTxtRepository _robotsTxtRepository;
        private readonly IRobotsTxtValidator _robotsTxtValidator;
        private readonly IRobotsTxtSitemapProvider _sitemapProvider;

        public RobotsTxtService(IRobotsTxtRepository robotsTxtRepository,
            IRobotsTxtValidator robotsTxtValidator,
            IRobotsTxtSitemapProvider sitemapProvider = null)
        {
            _robotsTxtRepository = robotsTxtRepository;
            _robotsTxtValidator = robotsTxtValidator;
            _sitemapProvider = sitemapProvider;
        }

        public string GetContent()
        {
            return _robotsTxtRepository.GetAll().FirstOrDefault()?.Content ?? string.Empty;
        }

        public string GetContentWithSitemaps(HttpRequest request)
        {
            string[] sitemaps = Array.Empty<string>();
            if (_sitemapProvider != null)
                sitemaps = _sitemapProvider.GetSitemapUrls(request).ToArray();

            //This will probably support multiple domains later on, but for now we can just take the only one
            var content = _robotsTxtRepository.GetAll().FirstOrDefault()?.Content ?? string.Empty;

            if (sitemaps.Length > 0)
            {
                var sitemapStringBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(content)) sitemapStringBuilder.Append("\n");
                foreach(var sitemap in sitemaps)
                {
                    sitemapStringBuilder.Append($"Sitemap: {sitemap}\n");
                }
                content += sitemapStringBuilder.ToString();
            }

            return content;
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
