using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SeoCheckOptionsTests
    {
        private static SeoCheckDescriptor Descriptor => new()
        {
            Alias = "Sample.Options",
            Name = "Options sample",
            Category = SeoCheckCategory.OnPage,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int("MaxLength", "Maximum length", 60),
                SeoCheckOption.Bool("Strict", "Strict mode", false),
                SeoCheckOption.Text("Suffix", "Suffix", " | Site"),
                SeoCheckOption.TextList("Ignore", "Ignored paths", new[] { "/admin" }))
        };

        [Test]
        public void Resolve_FallsBackToWhatTheCheckDeclared()
        {
            var options = SeoCheckOptionsResolver.Resolve(Descriptor);

            Assert.Multiple(() =>
            {
                Assert.That(options.Get("MaxLength", 0), Is.EqualTo(60));
                Assert.That(options.Get("Strict", true), Is.False);
                Assert.That(options.Get("Suffix", ""), Is.EqualTo(" | Site"));
                Assert.That(options.Get("Ignore", Array.Empty<string>()), Is.EqualTo(new[] { "/admin" }));
            });
        }

        [Test]
        public void Resolve_AppliesLayersInOrder_WithTheLastWinning()
        {
            var fromAppSettings = new Dictionary<string, object> { ["MaxLength"] = 70, ["Strict"] = true };
            var fromRun = new Dictionary<string, object> { ["MaxLength"] = 80 };

            var options = SeoCheckOptionsResolver.Resolve(Descriptor, fromAppSettings, fromRun);

            Assert.Multiple(() =>
            {
                Assert.That(options.Get("MaxLength", 0), Is.EqualTo(80), "the run override wins");
                Assert.That(options.Get("Strict", false), Is.True, "the appsettings layer still applies");
            });
        }

        [Test]
        public void Resolve_IgnoresValuesTheCheckNeverDeclared()
        {
            //An option the check does not read is almost always a typo, and silently accepting
            //it would hide the mistake.
            var options = SeoCheckOptionsResolver.Resolve(Descriptor,
                new Dictionary<string, object> { ["MxLength"] = 5 });

            Assert.That(options.Get("MaxLength", 0), Is.EqualTo(60));
        }

        [Test]
        public void Resolve_IgnoresNullOverrides()
        {
            var options = SeoCheckOptionsResolver.Resolve(Descriptor,
                new Dictionary<string, object> { ["MaxLength"] = null! },
                null);

            Assert.That(options.Get("MaxLength", 0), Is.EqualTo(60));
        }

        [Test]
        public void Get_CoercesValuesThatArrivedAsStrings()
        {
            //Configuration binding hands everything over as text.
            var options = SeoCheckOptionsResolver.Resolve(Descriptor,
                new Dictionary<string, object> { ["MaxLength"] = "75", ["Strict"] = "true" });

            Assert.Multiple(() =>
            {
                Assert.That(options.Get("MaxLength", 0), Is.EqualTo(75));
                Assert.That(options.Get("Strict", false), Is.True);
            });
        }

        [Test]
        public void Get_FallsBackWhenAValueCannotBeCoerced()
        {
            var options = SeoCheckOptionsResolver.Resolve(Descriptor,
                new Dictionary<string, object> { ["MaxLength"] = "not a number" });

            Assert.That(options.Get("MaxLength", 42), Is.EqualTo(42));
        }

        [Test]
        public void Resolve_ReturnsTheSharedEmptyInstanceForACheckWithNoOptions()
        {
            var descriptor = new SeoCheckDescriptor
            {
                Alias = "Sample.NoOptions",
                Name = "No options",
                Category = SeoCheckCategory.Other
            };

            Assert.That(SeoCheckOptionsResolver.Resolve(descriptor), Is.SameAs(SeoCheckOptions.Empty));
        }
    }

    [TestFixture]
    public class SeoCheckMessageFormatterTests
    {
        private readonly DefaultSeoCheckMessageFormatter _formatter = new();

        private static SeoCheckDescriptor Descriptor => new()
        {
            Alias = "Sample.Message",
            Name = "Title length",
            Category = SeoCheckCategory.OnPage,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Something is wrong with the title.",
                ["TooLong"] = "Title is {Length} characters, maximum is {Max}."
            }
        };

        [Test]
        public void Format_SubstitutesTheIssueData()
        {
            var issue = Report("TooLong", ("Length", 93), ("Max", 60));

            Assert.That(_formatter.Format(issue, Descriptor),
                Is.EqualTo("Title is 93 characters, maximum is 60."));
        }

        [Test]
        public void Format_FallsBackToTheDefaultTemplateForAnUnknownVariant()
        {
            var issue = Report("SomethingElse");

            Assert.That(_formatter.Format(issue, Descriptor), Is.EqualTo("Something is wrong with the title."));
        }

        [Test]
        public void Format_FallsBackToTheCheckNameWhenThereIsNoTemplate()
        {
            var descriptor = new SeoCheckDescriptor
            {
                Alias = "Sample.Bare",
                Name = "Bare check",
                Category = SeoCheckCategory.Other
            };

            Assert.That(_formatter.Format(Report(null), descriptor), Is.EqualTo("Bare check"));
        }

        [Test]
        public void Format_LeavesAnUnknownPlaceholderVisible()
        {
            //A hole in the sentence hides the mistake; leaving the token shows it.
            var issue = Report("TooLong", ("Length", 93));

            Assert.That(_formatter.Format(issue, Descriptor),
                Is.EqualTo("Title is 93 characters, maximum is {Max}."));
        }

        [Test]
        public void Format_HandlesAMalformedTemplateWithoutThrowing()
        {
            var descriptor = new SeoCheckDescriptor
            {
                Alias = "Sample.Malformed",
                Name = "Malformed",
                Category = SeoCheckCategory.Other,
                MessageTemplates = new Dictionary<string, string> { [""] = "Unclosed {Length" }
            };

            Assert.That(_formatter.Format(Report(null, ("Length", 1)), descriptor), Is.EqualTo("Unclosed {Length"));
        }

        private static SeoCheckIssue Report(string? variant, params (string Key, object Value)[] data)
        {
            var sink = new RecordingIssueSink();
            var context = new SeoPageCheckContext(
                CrawledResourceBuilder.A().Build(),
                Descriptor,
                SeoCheckOptions.Empty,
                new FakeRunContext(),
                sink);

            var builder = context.Report(variant);
            foreach (var (key, value) in data)
                builder = builder.Data(key, value);

            return sink.Issues[0];
        }
    }
}
