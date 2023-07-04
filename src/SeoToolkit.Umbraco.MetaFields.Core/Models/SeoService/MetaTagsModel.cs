using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService
{
    public class MetaTagsModel
    {
        public Dictionary<ISeoField, object> Fields { get; }

        public MetaTagsModel(Dictionary<ISeoField, object> fields = null)
        {
            Fields = fields ?? new Dictionary<ISeoField, object>();
        }

        public T GetValue<T>(string alias) where T : class
        {
            var keyValue = Fields.FirstOrDefault(it => it.Key.Alias.Equals(alias));
            return keyValue.Key is null ? default : keyValue.Value as T;
        }

        public void SetValue<T>(string alias, T value)
        {
            var (key, _) = Fields.FirstOrDefault(it => it.Key.Alias.Equals(alias));
            if (key is null)
            {
                throw new ArgumentException($"No SEO field with alias {alias} found!");
            }

            Fields[key] = value;
        }

        public bool ContainsField(string alias)
        {
            return Fields.FirstOrDefault(it => it.Key.Alias.Equals(alias)).Key != null;
        }

        public string Title
        {
            get => GetValue<string>(SeoFieldAliasConstants.Title);
            set => SetValue(SeoFieldAliasConstants.Title, value);
        }

        public string MetaDescription
        {
            get => GetValue<string>(SeoFieldAliasConstants.MetaDescription);
            set => SetValue(SeoFieldAliasConstants.MetaDescription, value);
        }

        public string OpenGraphTitle
        {
            get => GetValue<string>(SeoFieldAliasConstants.OpenGraphTitle);
            set => SetValue(SeoFieldAliasConstants.OpenGraphTitle, value);
        }

        public string OpenGraphDescription
        {
            get => GetValue<string>(SeoFieldAliasConstants.OpenGraphDescription);
            set => SetValue(SeoFieldAliasConstants.OpenGraphDescription, value);
        }

        public string OpenGraphImage
        {
            get => GetValue<string>(SeoFieldAliasConstants.OpenGraphImage);
            set => SetValue(SeoFieldAliasConstants.OpenGraphImage, value);
        }

        public string CanonicalUrl
        {
            get => GetValue<string>(SeoFieldAliasConstants.CanonicalUrl);
            set => SetValue(SeoFieldAliasConstants.CanonicalUrl, value);
        }

        public string[] Robots
        {
            get => GetValue<string[]>(SeoFieldAliasConstants.Robots);
            set => SetValue(SeoFieldAliasConstants.Robots, value);
        }

        public string Keywords
        {
            get => GetValue<string>(SeoFieldAliasConstants.Keywords);
            set => SetValue(SeoFieldAliasConstants.Keywords, value);
        }
    }
}
