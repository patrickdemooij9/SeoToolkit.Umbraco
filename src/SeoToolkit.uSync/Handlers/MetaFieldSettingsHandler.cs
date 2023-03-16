using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using uSync.BackOffice;


using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Strings;

using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.Core;
using uSyncConstants = uSync.BackOffice.uSyncConstants;

namespace SeoToolkit.uSync.Handlers
{
    [SyncHandler("seoToolkitTest", "seoToolkit DocTypes", "seoToolkitContentTypes", uSyncConstants.Priorites.ContentTypes,
        IsTwoPass = true, Icon = "icon-item-arrangement", EntityType = Constants.UdiEntityType.DocumentType)]
    public class MetaFieldSettingsHandler : SeoToolKitSyncHandlerBase<DocumentTypeSettingsDto>, ISyncHandler
    {
        private readonly IRepository<DocumentTypeSettingsDto> _repository;

        //TODO should ideally be implemented
        protected override IEnumerable<uSyncAction> DeleteMissingItems(DocumentTypeSettingsDto parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return new List<uSyncAction>();
        }
        

        protected override IEnumerable<DocumentTypeSettingsDto> GetChildItems(DocumentTypeSettingsDto parent)
        {
            return parent == null! ? _repository.GetAll().ToList() : new List<DocumentTypeSettingsDto>();
        }

        protected override string GetItemName(DocumentTypeSettingsDto item)
        {
            return item.Content.Name ?? "";
        }

        public IEnumerable<uSyncAction> ProcessPostImport(string folder, IEnumerable<uSyncAction> actions, HandlerSettings config)
        {
            return new List<uSyncAction>();
        }

        public MetaFieldSettingsHandler(IRepository<DocumentTypeSettingsDto> repository, ILogger<MetaFieldSettingsHandler> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, SyncFileService syncFileService, uSyncEventService mutexService, uSyncConfigService uSyncConfig, ISyncItemFactory itemFactory) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _repository = repository;
        }
    }
}