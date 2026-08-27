#nullable enable
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Base for checks that read parsed markup.
    /// <para>
    /// Handles the two things every such check would otherwise repeat: skipping resources that
    /// are not HTML pages, and null-checking the parsed facts. What is left is the actual rule.
    /// </para>
    /// <para>
    /// For a markup check that has to await something, derive from
    /// <see cref="SeoPageCheckBase"/> and read <c>context.Facts</c> directly.
    /// </para>
    /// </summary>
    public abstract class HtmlPageCheckBase : SeoPageCheckBase
    {
        protected sealed override void Check(in SeoPageCheckContext context)
        {
            var facts = context.Facts;

            //No parsed markup means the fetch failed or the resource was not a page. Either way
            //that is someone else's finding to report, not this check's.
            if (facts is null) return;

            Check(facts, in context);
        }

        protected abstract void Check(PageFacts facts, in SeoPageCheckContext context);
    }
}
