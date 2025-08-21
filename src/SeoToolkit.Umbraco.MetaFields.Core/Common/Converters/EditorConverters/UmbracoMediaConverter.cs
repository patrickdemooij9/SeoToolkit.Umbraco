using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class UmbracoMediaConverter : IEditorValueConverter
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public UmbracoMediaConverter(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public object ConvertDatabaseToObject(object value)
        {
            if (!Guid.TryParse(value?.ToString(), out var id))
                return null;

            var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return ctx.UmbracoContext.Media.GetById(id);
        }

        public object ConvertEditorToDatabaseValue(object value)
        {
            var images = JsonHelpers.DeserializeArray<MediaEditorModel>(value?.ToString());
            if (images is null || images.Length == 0)
                return null;

            return images[0].MediaKey;
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (value is not IPublishedContent content)
                return null;

            return new MediaEditorModel[]
            {
                new MediaEditorModel
                {
                    Key = Guid.NewGuid(),
                    MediaKey = content.Key
                }
            };
        }

        public bool IsEmpty(object value)
        {
            return value is null;
        }

        private class MediaEditorModel
        {
            [JsonPropertyName("key")]
            public Guid Key { get; set; }

            [JsonPropertyName("mediaKey")]
            public Guid MediaKey { get; set; }
        }
    }
}
