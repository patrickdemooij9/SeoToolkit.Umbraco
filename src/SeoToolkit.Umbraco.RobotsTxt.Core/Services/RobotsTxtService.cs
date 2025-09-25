using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Startup;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Services
{
    public class RobotsTxtService : IRobotsTxtService
    {
        private readonly IRobotsTxtRepository _robotsTxtRepository;
        private readonly IRobotsTxtValidator _robotsTxtValidator;
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly IRobotsTxtSitemapProvider _sitemapProvider;

        public RobotsTxtService(IRobotsTxtRepository robotsTxtRepository,
            IRobotsTxtValidator robotsTxtValidator,
            ISeoDomainResolver seoDomainResolver,
            IRobotsTxtSitemapProvider sitemapProvider = null)
        {
            _robotsTxtRepository = robotsTxtRepository;
            _robotsTxtValidator = robotsTxtValidator;
            _seoDomainResolver = seoDomainResolver;
            _sitemapProvider = sitemapProvider;
        }

        public string GetContent(int? domainId = null)
        {
            return _robotsTxtRepository.GetAll().FirstOrDefault(it => it.DomainId == domainId)?.Content ?? string.Empty;
        }

        public string GetContentWithSitemaps(HttpRequest request)
        {
            string[] sitemaps = [];
            if (_sitemapProvider != null)
                sitemaps = [.. _sitemapProvider.GetSitemapUrls(request)];

            var seoDomain = _seoDomainResolver.ResolveSeoDomain(new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"));
            var allRobotsTxt = _robotsTxtRepository.GetAll();

            var useDomainSpecific = seoDomain != null && seoDomain.HasFunctionality($"Module.{RobotsTxtTreeSection.RobotsTxtSectionGuid}");
            var robotsTxt = allRobotsTxt.FirstOrDefault(it => useDomainSpecific ? it.DomainId == seoDomain.Id : it.DomainId is null);

            var content = _robotsTxtRepository.GetAll().FirstOrDefault(it => it.DomainId == seoDomain?.Id)?.Content ?? string.Empty;

            if (sitemaps.Length > 0)
            {
                var sitemapStringBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(content)) sitemapStringBuilder.Append('\n');
                foreach(var sitemap in sitemaps)
                {
                    sitemapStringBuilder.Append($"Sitemap: {sitemap}\n");
                }
                content += sitemapStringBuilder.ToString();
            }

            return content;
        }

        public void SetContent(string content, int? domainId = null)
        {
            var model = _robotsTxtRepository.GetAll().FirstOrDefault(it => it.DomainId == domainId);
            if (model is null)
                model = new RobotsTxtModel();

            model.Content = content;
            model.DomainId = domainId;
            _robotsTxtRepository.Update(model);
        }

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            return _robotsTxtValidator.Validate(content);
        }
    }
}
