using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.PostModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit MetaFields")]
    [BackOfficeRoute("seoToolkitMetaFieldsSettings")]
    public class MetaFieldsSettingsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _seoFieldCollection;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentTypeService _contentTypeService;

        public MetaFieldsSettingsController(IMetaFieldsSettingsService documentTypeSettingsService,
            SeoFieldCollection seoFieldCollection,
            IUmbracoMapper umbracoMapper,
            IUmbracoContextFactory umbracoContextFactory,
            IContentTypeService contentTypeService)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
            _seoFieldCollection = seoFieldCollection;
            _umbracoMapper = umbracoMapper;
            _umbracoContextFactory = umbracoContextFactory;
            _contentTypeService = contentTypeService;
        }

        [HttpGet("metaFieldsSettings")]
        [ProducesResponseType(typeof(DocumentTypeSettingsViewModel), 200)]
        public IActionResult Get(Guid? nodeId)
        {
            DocumentTypeSettingsContentViewModel content = null;
            if (nodeId != null)
            {
                var contentType = _contentTypeService.Get(nodeId.Value);
                if (contentType != null)
                {
                    var model = _documentTypeSettingsService.Get(contentType.Key);
                    var fields = _seoFieldCollection.GetAll().Select(it =>
                    {
                        var fieldVm = model != null
                            ? new SeoFieldViewModel(it, model.Get(it.Alias))
                            : new SeoFieldViewModel(it);

                        if (fieldVm.Editor != null)
                        {
                            var config = new Dictionary<string, object>(
                                fieldVm.Editor.Config ?? new Dictionary<string, object>())
                            {
                                ["nodeGuid"] = contentType.Key.ToString(),
                                ["ownerType"] = "documentType"
                            };
                            fieldVm.Editor.Config = config;
                        }

                        return fieldVm;
                    }).ToArray();

                    content = model != null
                        ? new DocumentTypeSettingsContentViewModel(model, fields)
                        : new DocumentTypeSettingsContentViewModel(fields);
                }
            }
            if (content is null)
            {
                content = new DocumentTypeSettingsContentViewModel(_seoFieldCollection.GetAll().Select(it => new SeoFieldViewModel(it)).ToArray());
            }

            return Ok(new DocumentTypeSettingsViewModel
            {
                ContentModel = content,
            });
        }

        [HttpGet("metaFieldsAdditionalFields")]
        [ProducesResponseType(typeof(FieldItemViewModel[]), 200)]
        public IActionResult GetAdditionalFields()
        {
            return Ok(_documentTypeSettingsService.GetAdditionalFieldItems());
        }

        [HttpPost("metaFieldsSettings")]
        public IActionResult Save(DocumentTypeSettingsPostViewModel postModel)
        {
            if (postModel is null) return Ok();

            _documentTypeSettingsService.Set(_umbracoMapper.Map<DocumentTypeSettingsPostViewModel, DocumentTypeSettingsDto>(postModel));
            return Ok();
        }
    }
}
