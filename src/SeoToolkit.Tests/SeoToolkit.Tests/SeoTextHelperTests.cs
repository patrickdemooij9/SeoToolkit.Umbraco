using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Helpers;
using Umbraco.Cms.Core.Strings;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SeoTextHelperTests
    {
        [TestCase("<p>Fish &amp; chips</p>", "Fish & chips")]
        [TestCase("<p>Learn&#160;online</p>", "Learn online")]
        [TestCase("<p>One.</p><p>Two.</p>", "One. Two.")]
        [TestCase("<p>  Spread\r\n  over\tlines  </p>", "Spread over lines")]
        [TestCase("<p>&lt;b&gt; is bold</p>", "<b> is bold")]
        public void HtmlToPlainText_StripsTagsDecodesEntitiesAndCollapsesWhitespace(string html, string expected)
        {
            Assert.That(SeoTextHelper.HtmlToPlainText(html), Is.EqualTo(expected));
        }

        [Test]
        public void HtmlEncodedStringSeoConverter_DecodesEntities()
        {
            var converter = new HtmlEncodedStringSeoConverter();

            var result = converter.Convert(new HtmlEncodedString("<p>Health &amp; Fitness</p>"), null!, "metaDescription");

            Assert.That(result, Is.EqualTo("Health & Fitness"));
        }

        [Test]
        public void Truncate_LeavesTextThatFits()
        {
            Assert.That(SeoTextHelper.Truncate("Short enough.", 20), Is.EqualTo("Short enough."));
        }

        [Test]
        public void Truncate_EndsAtLastSentenceInSecondHalf()
        {
            const string text = "The first sentence is here. The second sentence runs well past the limit.";

            Assert.That(SeoTextHelper.Truncate(text, 40), Is.EqualTo("The first sentence is here."));
        }

        [Test]
        public void Truncate_FallsBackToWordBoundaryWithEllipsis()
        {
            const string text = "A single long sentence without any early full stop, that keeps going on";

            var result = SeoTextHelper.Truncate(text, 40);

            Assert.That(result, Is.EqualTo("A single long sentence without any…"));
            Assert.That(result.Length, Is.LessThanOrEqualTo(40));
        }

        [Test]
        public void Truncate_IgnoresFullStopInsideAWord()
        {
            const string text = "Price $4.50 each and more words";

            Assert.That(SeoTextHelper.Truncate(text, 12), Is.EqualTo("Price $4.50…"));
        }

        [Test]
        public void Truncate_HardCutsWhenThereIsNoSpace()
        {
            var result = SeoTextHelper.Truncate(new string('a', 50), 10);

            Assert.That(result, Is.EqualTo(new string('a', 9) + "…"));
        }
    }
}
