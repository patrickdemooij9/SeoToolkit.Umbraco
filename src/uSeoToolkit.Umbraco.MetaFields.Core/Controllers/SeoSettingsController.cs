using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.PostModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.ViewModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class SeoSettingsController : UmbracoAuthorizedApiController
    {
        private readonly ISeoService _seoService;
        private readonly IDocumentTypeSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _fieldCollection;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISeoValueService _seoValueService;
        private readonly ILogger<SeoSettingsController> _logger;

        public SeoSettingsController(ISeoService seoService,
            IDocumentTypeSettingsService documentTypeSettingsService,
            SeoFieldCollection fieldCollection,
            IUmbracoContextFactory umbracoContextFactory,
            ISeoValueService seoValueService,
            ILogger<SeoSettingsController> logger)
        {
            _seoService = seoService;
            _documentTypeSettingsService = documentTypeSettingsService;
            _fieldCollection = fieldCollection;
            _umbracoContextFactory = umbracoContextFactory;
            _seoValueService = seoValueService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(int nodeId, int contentTypeId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content.GetById(true, nodeId);
            if (content is null)
            {
                _logger.LogInformation("Could not find content by id {0}", nodeId);
                return NotFound();
            }

            var metaTags = _seoService.Get(content);
            var userValues = _seoValueService.GetUserValues(nodeId);

            return new JsonResult(new SeoSettingsViewModel
            {
                Fields = metaTags.Fields.Select(it =>
                {
                    var (key, value) = it;
                    var userValue = userValues.ContainsKey(key.Alias)
                        ? key.EditEditor.ValueConverter.ConvertObjectToEditorValue(key.EditEditor.ValueConverter.ConvertDatabaseToObject(userValues[key.Alias]))
                        : null;
                    return new SeoSettingsFieldViewModel
                    {
                        Alias = key.Alias,
                        Title = key.Title,
                        Description = key.Description,
                        Value = value?.ToString(),
                        UserValue = userValue,
                        EditView = key.EditEditor.View,
                        EditConfig = key.EditEditor.Config
                    };
                }).ToArray(),
                Previewers = new[] { new FieldPreviewerViewModel(new BaseTagsFieldPreviewer()) }
            });
        }

        [HttpPost]
        public IActionResult Save(SeoSettingsPostViewModel postModel)
        {
            var settings = _documentTypeSettingsService.Get(postModel.ContentTypeId);
            if (!settings.EnableSeoSettings)
                return BadRequest("Seo settings are turned off for this node!");

            var isDirty = false;
            var values = new Dictionary<string, object>();
            foreach (var (seoField, _) in settings.Fields)
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
                _seoValueService.AddValues(postModel.NodeId, values);

            return Get(postModel.NodeId, postModel.ContentTypeId);
        }
    }
}
