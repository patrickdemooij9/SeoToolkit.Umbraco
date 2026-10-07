using Microsoft.Extensions.DependencyInjection;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldSuggestions;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SeoFieldCollectionBuilderTests
    {
        [Test]
        public void Update_AppliesToMatchingFieldOnly()
        {
            var builder = new SeoFieldCollectionBuilder()
                .Add<SeoTitleField>()
                .Add<SeoDescriptionField>()
                .Update<SeoDescriptionField>(field =>
                {
                    var maxLength = field.Suggestions.OfType<SeoFieldMaxLengthSuggestion>().First();
                    maxLength.MaxLength = 155;
                    maxLength.TruncateFallbackValue = true;
                });

            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<ISeoKeyValueService>());
            builder.RegisterWith(services);
            var collection = services.BuildServiceProvider().GetRequiredService<SeoFieldCollection>();

            var description = collection.OfType<SeoDescriptionField>().Single()
                .Suggestions.OfType<SeoFieldMaxLengthSuggestion>().Single();
            var title = collection.OfType<SeoTitleField>().Single()
                .Suggestions.OfType<SeoFieldMaxLengthSuggestion>().Single();

            Assert.Multiple(() =>
            {
                Assert.That(description.MaxLength, Is.EqualTo(155));
                Assert.That(description.TruncateFallbackValue, Is.True);
                Assert.That(title.TruncateFallbackValue, Is.False);
            });
        }
    }
}
