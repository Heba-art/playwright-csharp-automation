using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlaywrightTests.Utils
{
    public class TestBase
    {
        protected IPlaywright _playwright = default!;
        protected IBrowser _browser = default!;
        protected IBrowserContext _context = default!;
        protected IPage _page = default!;

        // Defaults (can be overridden by appsettings.json or ENV)
        protected string _baseUrl = "https://demo.nopcommerce.com";
        private string _browserType = "chromium";
        private bool _headless = true;
        private int _viewportWidth = 1366;
        private int _viewportHeight = 768;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            // ---------- Read appsettings.json if present ----------
            var configPath = File.Exists("appsettings.json")
                ? "appsettings.json"
                : Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (File.Exists(configPath))
            {
                var json = await File.ReadAllTextAsync(configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("baseUrl", out var pUrl))
                    _baseUrl = pUrl.GetString() ?? _baseUrl;

                if (root.TryGetProperty("browser", out var pBrowser))
                    _browserType = pBrowser.GetString() ?? _browserType;

                if (root.TryGetProperty("headless", out var pHeadless))
                    _headless = pHeadless.GetBoolean();

                if (root.TryGetProperty("viewport", out var vp))
                {
                    if (vp.TryGetProperty("width", out var w)) _viewportWidth = w.GetInt32();
                    if (vp.TryGetProperty("height", out var h)) _viewportHeight = h.GetInt32();
                }
            }

            // ---------- ENV overrides (CI-friendly) ----------
            var envBrowser = Environment.GetEnvironmentVariable("BROWSER");
            if (!string.IsNullOrWhiteSpace(envBrowser))
                _browserType = envBrowser;

            var envBaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
            if (!string.IsNullOrWhiteSpace(envBaseUrl))
                _baseUrl = envBaseUrl;

            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            var envHeadless = Environment.GetEnvironmentVariable("HEADLESS");
            var resolvedHeadless = envHeadless switch
            {
                null when isCI => true, // force headless on CI if not explicitly set
                null => _headless,
                _ => envHeadless.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                     envHeadless.Equals("true", StringComparison.OrdinalIgnoreCase)
            };

            // ---------- Playwright startup ----------
            _playwright = await Playwright.CreateAsync();

            var typeLower = (_browserType ?? "chromium").ToLowerInvariant();
            IBrowserType browserType = typeLower switch
            {
                "firefox" => _playwright.Firefox,
                "webkit" => _playwright.Webkit,
                _ => _playwright.Chromium
            };

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = resolvedHeadless
            };

            // Helpful flags on Linux runners
            if (OperatingSystem.IsLinux() && (typeLower == "chromium" || typeLower == "firefox"))
                launchOptions.Args = new[] { "--no-sandbox" };

            _browser = await browserType.LaunchAsync(launchOptions);
            // NOTE: context/page are created per test in [SetUp]
        }

        [SetUp]
        public async Task SetUpTest()
        {
            // New isolated context for each test
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = _baseUrl,
                ViewportSize = new ViewportSize { Width = _viewportWidth, Height = _viewportHeight }
            });

            _page = await _context.NewPageAsync();

            var defaultActionTimeout = 30_000;
            if (int.TryParse(Environment.GetEnvironmentVariable("PW_TIMEOUT"), out var fromEnv))
                defaultActionTimeout = fromEnv;

            _context.SetDefaultTimeout(defaultActionTimeout);

            _page = await _context.NewPageAsync();
            _page.SetDefaultTimeout(defaultActionTimeout);

            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            _page.SetDefaultNavigationTimeout(isCI ? 60_000 : 45_000);



            // Start tracing (great for CI debugging)
            await _context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        [TearDown]
        public async Task TearDownTest()
        {
            Directory.CreateDirectory("artifacts/screenshots");
            Directory.CreateDirectory("artifacts/traces");

            var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
            var id = TestContext.CurrentContext.Test.ID;

            if (failed && _page is not null)
            {
                await _page.ScreenshotAsync(new()
                {
                    Path = $"artifacts/screenshots/{id}.png",
                    FullPage = true
                });
            }

            // Stop tracing (save file only on failure)
            if (_context is not null)
            {
                var tracePath = failed ? $"artifacts/traces/{id}.zip" : null;
                await _context.Tracing.StopAsync(new() { Path = tracePath });
            }

            // Close per-test context
            if (_context is not null)
                await _context.CloseAsync();
        }

        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            if (_browser is not null)
                await _browser.CloseAsync();

            _playwright?.Dispose();

            // Optional: cleanup any temp credentials created during tests
            var dir = TestContext.CurrentContext.WorkDirectory;
            foreach (var file in Directory.GetFiles(dir, "lastUser_*.json"))
            {
                try
                {
                    File.Delete(file);
                    TestContext.Out.WriteLine($"[CLEANUP] Deleted {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    TestContext.Out.WriteLine($"[CLEANUP-ERROR] {ex.Message}");
                }
            }
        }

        // Optional utility
        protected async Task PrintBrowserInfoAsync()
        {
            var ua = await _page.EvaluateAsync<string>("navigator.userAgent");
            TestContext.Out.WriteLine($"[INFO] UA: {ua}");
        }
    }
}
