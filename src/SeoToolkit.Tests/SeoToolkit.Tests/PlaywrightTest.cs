using Lucene.Net.Search;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SeoToolkit.Tests;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ExampleTest : PageTest
{
    private Process? _umbracoProcess;
    private string _baseUrl;

    private static string UmbracoProjectDir =>
        Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "SeoToolkit.Umbraco.Site"));

    [OneTimeSetUp]
    public async Task StartUmbraco()
    {
        _baseUrl = $"https://localhost:44324/";

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --no-build",
            WorkingDirectory = UmbracoProjectDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // tell Kestrel to bind to our chosen port
            Environment =
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Testing"
            }
        };

        _umbracoProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        _umbracoProcess.OutputDataReceived += (s, e) => { if (e.Data != null)
            {
                TestContext.Progress.WriteLine(e.Data);
            }
        };
        _umbracoProcess.ErrorDataReceived += (s, e) => { if (e.Data != null)
            {
                TestContext.Progress.WriteLine("ERR: " + e.Data);
            }
        };

        _umbracoProcess.Start();

        _umbracoProcess.BeginOutputReadLine();
        _umbracoProcess.BeginErrorReadLine();

        await Task.Delay(20000);
    }

    [OneTimeTearDown]
    public void StopUmbracoSite()
    {
        _umbracoProcess?.Kill(true);
    }

    [Test]
    public async Task TestIfDashboardLoads()
    {
        await Page.GotoAsync(_baseUrl + "umbraco");
        await Page.Locator("#username-input").PressSequentiallyAsync("admin@email.com");
        await Page.Locator("#password-input").PressSequentiallyAsync("Password123!");
        await Page.Locator("#button").ClickAsync();

        await Expect(Page).ToHaveURLAsync("https://localhost:44324/umbraco/section/content", new PageAssertionsToHaveURLOptions
        {
            Timeout = 10000
        });

        await Page.Locator("[data-mark=\"section-link:SeoToolkit\"]").ClickAsync();
        await Expect(Page).ToHaveURLAsync("https://localhost:44324/umbraco/section/SeoToolkit");
        await Expect(Page.Locator(".module-icon").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task GetStartedLink()
    {
        await Page.GotoAsync("https://playwright.dev");

        // Click the get started link.
        await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();

        // Expects page to have a heading with the name of Installation.
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
    }
}