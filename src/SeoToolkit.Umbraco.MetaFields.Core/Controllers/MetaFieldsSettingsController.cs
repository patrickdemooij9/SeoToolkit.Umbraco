﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.PostModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Api.Management.Controllers;

namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class MetaFieldsSettingsController : ManagementApiControllerBase
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _seoFieldCollection;
        private readonly IUmbracoMapper _umbracoMapper;

        public MetaFieldsSettingsController(IMetaFieldsSettingsService documentTypeSettingsService,
            SeoFieldCollection seoFieldCollection,
            IUmbracoMapper umbracoMapper)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
            _seoFieldCollection = seoFieldCollection;
            _umbracoMapper = umbracoMapper;
        }

        [HttpGet]
        public IActionResult Get(int nodeId)
        {
            var model = _documentTypeSettingsService.Get(nodeId);
            var content = model != null ? 
                new DocumentTypeSettingsContentViewModel(model, _seoFieldCollection.GetAll().Select(it => new SeoFieldViewModel(it, model.Get(it.Alias))).ToArray()) :
                new DocumentTypeSettingsContentViewModel(_seoFieldCollection.GetAll().Select(it => new SeoFieldViewModel(it)).ToArray());
            return new JsonResult(new DocumentTypeSettingsViewModel
            {
                ContentModel = content,
            });
        }

        [HttpGet]
        public IActionResult GetAdditionalFields()
        {
            return new JsonResult(_documentTypeSettingsService.GetAdditionalFieldItems());
        }

        [HttpPost]
        public IActionResult Save(DocumentTypeSettingsPostViewModel postModel)
        {
            _documentTypeSettingsService.Set(_umbracoMapper.Map<DocumentTypeSettingsPostViewModel, DocumentTypeSettingsDto>(postModel));
            return Ok();
        }
    }
}
