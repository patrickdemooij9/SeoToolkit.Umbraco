using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Notifications;
using SeoToolkit.Umbraco.RobotsTxt.Core.Startup;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Services
{
    public class RobotsTxtService : IRobotsTxtService
    {
        private readonly IRobotsTxtRepository _robotsTxtRepository;
        private readonly IRobotsTxtValidator _robotsTxtValidator;
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRobotsTxtSitemapProvider _sitemapProvider;

        public RobotsTxtService(IRobotsTxtRepository robotsTxtRepository,
            IRobotsTxtValidator robotsTxtValidator,
            ISeoDomainResolver seoDomainResolver,
            IEventAggregator eventAggregator,
            IRobotsTxtSitemapProvider sitemapProvider = null)
        {
            _robotsTxtRepository = robotsTxtRepository;
            _robotsTxtValidator = robotsTxtValidator;
            _seoDomainResolver = seoDomainResolver;
            _eventAggregator = eventAggregator;
            _sitemapProvider = sitemapProvider;
        }

        public RobotsTxtModel Get(Guid id)
        {
            return _robotsTxtRepository.Get(id);
        }

        public RobotsTxtModel[] GetAll()
        {
            return _robotsTxtRepository.GetAll().ToArray();
        }

        public string GetContent(Guid? domainId = null)
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

            var content = robotsTxt?.Content ?? string.Empty;

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

        public void Save(RobotsTxtModel model)
        {
            _robotsTxtRepository.Update(model);

            _eventAggregator.Publish(new RobotsTxtSavedNotification(model));
        }

        public void SetContent(string content, Guid? domainId = null)
        {
            var model = _robotsTxtRepository.GetAll().FirstOrDefault(it => it.DomainId == domainId);
            model ??= new RobotsTxtModel { Key = Guid.NewGuid() };

            model.Content = content;
            model.DomainId = domainId;
            _robotsTxtRepository.Update(model);

            _eventAggregator.Publish(new RobotsTxtSavedNotification(model));
        }

        public IEnumerable<RobotsTxtValidation> Validate(string content)
        {
            return _robotsTxtValidator.Validate(content);
        }
    }
}
