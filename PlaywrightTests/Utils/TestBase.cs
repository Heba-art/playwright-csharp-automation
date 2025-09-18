using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

// NOTE: This namespace should match your project structure.
namespace PlaywrightTests
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

        private ILocator RegisterLink => _page.Locator("a.ico-register");
        private ILocator MobileMenu => _page.Locator(".menu-toggle");
        private ILocator HeaderMenu => _page.Locator(".header-menu");

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
            // Start tracing (great for CI debugging)
            await _context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            _page = await _context.NewPageAsync();

            // --- MODIFICATION ---
            // Navigate to the base URL here so every test starts on the home page.
            await _page.GotoAsync(_baseUrl);
            await EnsurePageReadyAsync();

            // Set default timeouts
            var defaultActionTimeout = 60_000;
            if (int.TryParse(Environment.GetEnvironmentVariable("PW_TIMEOUT"), out var fromEnv))
                defaultActionTimeout = fromEnv;

            _context.SetDefaultTimeout(defaultActionTimeout);
            _page.SetDefaultTimeout(defaultActionTimeout);

            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            _page.SetDefaultNavigationTimeout(isCI ? Math.Max(defaultActionTimeout, 60_000)
                                                  : Math.Max(defaultActionTimeout, 45_000));

            Microsoft.Playwright.Assertions.SetDefaultExpectTimeout(defaultActionTimeout);


        }

        public async Task EnsurePageReadyAsync()
        {
            // 1) افتحي الصفحة + انتظري استقرار الشبكة
            await _page.GotoAsync(_baseUrl, new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000
            });
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 2) اقبلي الكوكيز إن وُجدت
            await AcceptCookiesIfPresentAsync();

            // 3) اغلقي أي شريط إشعار قد يغطي العناصر
            await DismissNotificationBarIfPresentAsync();

            // 4) تحقّق جاهزية الصفحة عبر عنصر موثوق (بديل .header-menu)
            var readyAnchor = _page.Locator(".header-logo a, a.ico-cart").First;
            await Assertions.Expect(readyAnchor).ToBeVisibleAsync(new()
            {
                Timeout = 60000
            });
        }

        private async Task AcceptCookiesIfPresentAsync()
        {
            // أزرار شائعة في الديمو
            var cookieBtn = _page.Locator("#eu-cookie-ok, .cookie-bar button, text=I agree").First;

            if (await IsQuicklyVisibleAsync(cookieBtn, 1000))
            {
                await cookieBtn.ClickAsync();
                await _page.WaitForTimeoutAsync(200);
            }
        }

        private async Task DismissNotificationBarIfPresentAsync()
        {
            var bar = _page.Locator("#bar-notification");
            if (await IsQuicklyVisibleAsync(bar, 1000))
            {
                var close = bar.Locator(".close, .close-notification").First;
                if (await IsQuicklyVisibleAsync(close, 500))
                    await close.ClickAsync();

                await bar.WaitForAsync(new()
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = 5000
                });
            }
        }

        // Helper بدون أي API قديم/مهجور
        private static async Task<bool> IsQuicklyVisibleAsync(ILocator locator, int timeoutMs = 800)
        {
            try
            {
                await locator.WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });
                return true;
            }
            catch (TimeoutException) { return false; }
            catch (PlaywrightException) { return false; }
        }


        [TearDown]
        public async Task TearDownTest()
        {// --- تعديل مهم: منطق جديد وموثوق لحفظ الآثار ---
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                var testId = TestContext.CurrentContext.Test.ID;
                var screenshotPath = Path.Combine("artifacts", "screenshots", $"{testId}.png");
                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                await _page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
            }

            // ضع إيقاف التتبع في try-catch لتجنب الخطأ
            try
            {
                var tracePath = Path.Combine("artifacts", "traces", $"{TestContext.CurrentContext.Test.ID}.zip");
                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    await _context.Tracing.StopAsync(new() { Path = tracePath });
                    TestContext.AddTestAttachment(tracePath, "Playwright Trace on failure");
                }
                else
                {
                    await _context.Tracing.StopAsync(); // أوقف التتبع بدون حفظ الملف
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Could not stop tracing: {ex.Message}");
            }

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