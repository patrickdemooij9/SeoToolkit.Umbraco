using Lucene.Net.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Common.Core.Startup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkit/tree/info")]
    public class SeoToolkitTreeController : SeoToolkitAuthenticatedControllerBase
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        private readonly SeoTreeSectionCollection _seoTreeSections;
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly IDomainService _domainService;
        private readonly SeoKeyValueSettingCollection _seoKeyValueSettings;
        private readonly ISettingsService<GlobalConfig> _config;

        private Guid _domainGuid = new Guid("ab248b43-9757-432a-9821-22f9eeb513e7");
        private Guid _settingsGuid = new Guid("5ed58cb7-2ec2-4c97-be5b-506d6189086f");

        public SeoToolkitTreeController(SeoTreeSectionCollection seoTreeSections, ISeoDomainsService seoDomainsService, IDomainService domainService, SeoKeyValueSettingCollection seoKeyValueSettings, ISettingsService<GlobalConfig> config)
        {
            _seoTreeSections = seoTreeSections;
            _seoDomainsService = seoDomainsService;
            _domainService = domainService;
            _seoKeyValueSettings = seoKeyValueSettings;
            _config = config;
        }

        [HttpGet("root")]
        [ProducesResponseType(typeof(PagedViewModel<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedViewModel<SeoToolkitTreeItemApiModel>>> GetRoot(int skip = 0, int take = 100)
        {
            var hasDomainSpecific = _seoTreeSections.Any(it => it.CanBeDomainSpecific);
            var items = _seoTreeSections.Select(it => new SeoToolkitTreeItemApiModel
            {
                Id = it.Id.ToString(),
                Name = it.Name
            }).ToList();

            if (_seoKeyValueSettings.Count > 0)
            {
                items.Add(new SeoToolkitTreeItemApiModel
                {
                    Id = _settingsGuid.ToString(),
                    Name = "Settings",
                });
            }

            var seoDomains = _seoDomainsService.GetAll();
            var umbracoDomains = await FindMissingUmbracoDomains(seoDomains);
            if (hasDomainSpecific && (umbracoDomains.Length > 0 || seoDomains.Length > 0))
            {
                items.Add(new SeoToolkitTreeItemApiModel
                {
                    Id = _domainGuid.ToString(),
                    Name = "Domains",
                    HasChildren = (_config.GetSettings().SyncContentDomains && umbracoDomains.Length > 0) || seoDomains.Length > 0
                });
            }

            var result = new PagedViewModel<SeoToolkitTreeItemApiModel>()
            {
                Items = items,
                Total = items.Count
            };

            return Ok(result);
        }

        [HttpGet("children")]
        [ProducesResponseType(typeof(PagedViewModel<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedViewModel<SeoToolkitTreeItemApiModel>>> GetChildren(string parentUnique, int skip = 0, int take = 100)
        {
            if (Guid.TryParse(parentUnique, out var resultGuid) && resultGuid == _domainGuid)
            {
                var allItems = _seoDomainsService.GetAll();
                var items = allItems.Skip(skip).Take(take).Select(it => new SeoToolkitTreeItemApiModel
                {
                    Id = $"{_domainGuid}~{it.Id}",
                    Name = it.Name,
                    HasChildren = GetSectionsForDomain(it.Id!.Value).Length > 0
                }).ToList();
                if (_config.GetSettings().SyncContentDomains)
                {
                    var domains = (await FindMissingUmbracoDomains(allItems))
                        .Select(it => new SeoToolkitTreeItemApiModel
                        {
                            Id = $"{_domainGuid}~d~{it.Id}",
                            Name = it.DomainName.Replace("https://", ""),
                            HasChildren = false,
                            IsDraft = true
                        }).ToArray();

                    if (domains.Length > 0)
                        items.AddRange(domains);
                }

                return Ok(new PagedViewModel<SeoToolkitTreeItemApiModel>()
                {
                    Items = items,
                    Total = allItems.Length
                });
            }
            else if (parentUnique.StartsWith($"{_domainGuid}~"))
            {
                if (Guid.TryParse(parentUnique.Replace($"{_domainGuid}~", ""), out var domainId))
                {
                    var sections = GetSectionsForDomain(domainId);
                    return new PagedViewModel<SeoToolkitTreeItemApiModel>
                    {
                        Items = sections.Select(it => new SeoToolkitTreeItemApiModel
                        {
                            Id = $"{it.Id}~{domainId}".ToLower(),
                            Name = it.Name,
                            ParentId = parentUnique,
                        }).ToArray(),
                        Total = sections.Length
                    };
                }
            }

            var result = new PagedViewModel<SeoToolkitTreeItemApiModel>()
            {
                Items = [],
                Total = 0
            };

            return Ok(result);
        }

        [HttpGet("ancestors")]
        [ProducesResponseType(typeof(IEnumerable<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<SeoToolkitTreeItemApiModel>> GetAncestors(Guid descendantId)
        {
            return Ok(Enumerable.Empty<SeoToolkitTreeItemApiModel>());
        }

        private ISeoTreeSection[] GetSectionsForDomain(Guid domainId)
        {
            var domain = _seoDomainsService.GetAll().FirstOrDefault(it => it.Id == domainId);
            if (domain is null) return [];
            var sections = _seoTreeSections.Where(it => it.CanBeDomainSpecific && domain.Settings.ContainsKey($"Module.{it.Id}")).ToList();
            if (_seoKeyValueSettings.Count > 0)
            {
                sections.Add(new SeoToolkitSettingsSection());
            }

            return sections.ToArray();
        }

        private async Task<IDomain[]> FindMissingUmbracoDomains(SeoDomainCollection[] seoDomains)
        {
            var currentlyUsedDomains = seoDomains.SelectMany(it => it.DomainIds).Distinct().ToArray();
            var domains = await _domainService.GetAllAsync(false);
            return [.. domains.Where(it => !it.DomainName.StartsWith('/') && !currentlyUsedDomains.Contains(it.Id))];
        }
    }
}
