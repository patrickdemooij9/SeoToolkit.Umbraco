using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsValue.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.PostModels;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkitMetaFields")]
    [BackOfficeRoute("seoToolkitMetaFields")]
    public class MetaFieldsController : SeoToolkitControllerBase
    {
        private readonly IMetaFieldsService _seoService;
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _fieldCollection;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IMetaFieldsValueService _seoValueService;
        private readonly ILogger<MetaFieldsController> _logger;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly SeoGroupCollection _groupCollection;
        private readonly ISettingsService<MetaFieldsConfigModel> _settingsService;

        public MetaFieldsController(IMetaFieldsService seoService,
            IMetaFieldsSettingsService documentTypeSettingsService,
            SeoFieldCollection fieldCollection,
            IUmbracoContextFactory umbracoContextFactory,
            IMetaFieldsValueService seoValueService,
            ILogger<MetaFieldsController> logger,
            IVariationContextAccessor variationContextAccessor,
            ILocalizationService localizationService,
            ISeoSettingsService seoSettingsService,
            SeoGroupCollection groupCollection,
            ISettingsService<MetaFieldsConfigModel> settingsService)
        {
            _seoService = seoService;
            _documentTypeSettingsService = documentTypeSettingsService;
            _fieldCollection = fieldCollection;
            _umbracoContextFactory = umbracoContextFactory;
            _seoValueService = seoValueService;
            _logger = logger;
            _variationContextAccessor = variationContextAccessor;
            _localizationService = localizationService;
            _seoSettingsService = seoSettingsService;
            _groupCollection = groupCollection;
            _settingsService = settingsService;
        }

        [HttpGet("metaFields")]
        [ProducesResponseType(typeof(MetaFieldsSettingsViewModel), 200)]
        public IActionResult Get(Guid nodeGuid, string culture)
        {
            EnsureLanguage(culture);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content.GetById(true, nodeGuid);

            var metaTags = content is null ? _seoService.GetEmpty() : _seoService.Get(content, false);
            if (metaTags is null)
                return Ok(new MetaFieldsSettingsViewModel());

            var userValues = content is null ? new Dictionary<string, object>() : _seoValueService.GetUserValues(content.Id);

            return Ok(new MetaFieldsSettingsViewModel
            {
                Groups = _groupCollection.Select(it => new SeoFieldGroupViewModel(it)).ToArray(),
                Fields = metaTags.Fields.Where(it => it.Key.EditEditor != null).Select(it =>
                {
                    var (key, value) = it;
                    var userValue = userValues.ContainsKey(key.Alias)
                        ? key.EditEditor.ValueConverter.ConvertObjectToEditorValue(key.EditEditor.ValueConverter.ConvertDatabaseToObject(userValues[key.Alias]))
                        : null;

                    var humanReadableValue = value is string[] ? string.Join(", ", value as string[]) : value;
                    return new SeoSettingsFieldViewModel
                    {
                        Alias = key.Alias,
                        Title = key.Title,
                        Description = key.Description,
                        GroupAlias = key.GroupAlias,
                        Value = humanReadableValue?.ToString(),
                        UserValue = userValue,
                        EditView = key.EditEditor.View,
                        EditConfig = key.EditEditor.Config
                    };
                }).ToArray(),
                Previewers = new[] { new FieldPreviewerViewModel(new MetaFieldsPreviewer()), new FieldPreviewerViewModel(new SocialMediaPreviewer()) }
            });
        }

        [HttpPost("metaFields")]
        [ProducesResponseType(typeof(MetaFieldsSettingsPostViewModel), 200)]
        public IActionResult Save(MetaFieldsSettingsPostViewModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content.GetById(true, postModel.NodeId);
            if (content is null)
            {
                return BadRequest($"Cannot find content by id: {postModel.NodeId}");
            }

            if (!_seoSettingsService.IsEnabled(content.ContentType))
                return BadRequest("SEO settings are turned off for this node!");

            EnsureLanguage(postModel.Culture);
            var isDirty = false;
            var values = new Dictionary<string, object>();
            foreach (var seoField in _fieldCollection)
            {
                if (!postModel.UserValues.ContainsKey(seoField.Alias))
                {
                    values.Add(seoField.Alias, null);
                    continue;
                }

                var userValue = postModel.UserValues[seoField.Alias];

                values.Add(seoField.Alias, seoField.EditEditor.ValueConverter.ConvertEditorToDatabaseValue(userValue));
                isDirty = true;
            }
            if (isDirty)
            {
                _seoValueService.AddValues(content.Id, values);
            }

            return Get(postModel.NodeId, postModel.Culture);
        }

        [HttpGet("imagePreview")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult ImagePreview(Guid mediaId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var mediaItem = ctx.UmbracoContext.Media.GetById(true, mediaId);
            if (mediaItem is null)
            {
                return NotFound();
            }
            var converter = new PublishedContentSeoValueConverter(_settingsService);
            return Ok(converter.Convert(mediaItem));
        }

        private void EnsureLanguage(string culture)
        {
            if (!string.IsNullOrWhiteSpace(culture) && culture != "invariant")
                _variationContextAccessor.VariationContext = new VariationContext(culture);
            else
                _variationContextAccessor.VariationContext = new VariationContext(_localizationService.GetDefaultLanguageIsoCode());
        }
    }
}
