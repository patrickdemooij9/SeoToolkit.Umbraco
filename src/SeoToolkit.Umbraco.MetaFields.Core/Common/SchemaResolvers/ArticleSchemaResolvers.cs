using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    /// <summary>
    /// Base resolver for the article types that Google supports: Article, BlogPosting and NewsArticle.
    /// </summary>
    public abstract class ArticleSchemaResolverBase<TArticle> : ISchemaResolver
        where TArticle : Article, new()
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        protected ArticleSchemaResolverBase(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public abstract string Name { get; }
        public abstract string Alias { get; }

        public SchemaProperty[] Properties =>
            [
                new("headline", "Headline", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("image", "Image", "Umb.PropertyEditorUi.MediaPicker", valueConverter: _mediaConverter),
                new("datePublished", "Date Published", "Umb.PropertyEditorUi.TextBox"),
                new("dateModified", "Date Modified", "Umb.PropertyEditorUi.TextBox"),
                SchemaValueHelper.NestedSchemaProperty("author", "Author", "person", "organization"),
                SchemaValueHelper.NestedSchemaProperty("publisher", "Publisher", "organization", "localBusiness", "person")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var url = values.GetUri("url");
            var image = values.GetUri("image");

            return new TArticle
            {
                Headline = values.GetString("headline"),
                Description = values.GetString("description"),
                MainEntityOfPage = url is null ? default : new Values<ICreativeWork, Uri>(url),
                Image = image is null ? default : new Values<IImageObject, Uri>(image),
                DatePublished = values.GetDate("datePublished"),
                DateModified = values.GetDate("dateModified"),
                Author = values.GetOrganizationsOrPersons("author"),
                Publisher = values.GetOrganizationsOrPersons("publisher")
            };
        }
    }

    public class ArticleSchemaResolver : ArticleSchemaResolverBase<Article>
    {
        public ArticleSchemaResolver(UmbracoMediaConverter mediaConverter) : base(mediaConverter)
        {
        }

        public override string Name => "Article";
        public override string Alias => "article";
    }

    public class BlogPostingSchemaResolver : ArticleSchemaResolverBase<BlogPosting>
    {
        public BlogPostingSchemaResolver(UmbracoMediaConverter mediaConverter) : base(mediaConverter)
        {
        }

        public override string Name => "Blog Posting";
        public override string Alias => "blogPosting";
    }

    public class NewsArticleSchemaResolver : ArticleSchemaResolverBase<NewsArticle>
    {
        public NewsArticleSchemaResolver(UmbracoMediaConverter mediaConverter) : base(mediaConverter)
        {
        }

        public override string Name => "News Article";
        public override string Alias => "newsArticle";
    }
}
