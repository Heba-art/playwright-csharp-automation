using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlaywrightTests
{
    public class TestBase
    {
        protected IPlaywright _playwright = default!;
        protected IBrowser _browser = default!;
        protected IBrowserContext _context = default!;
        protected IPage _page = default!;

        // Defaults (قابلة للتهيئة من appsettings.json أو ENV)
        protected string _baseUrl = "https://demo.nopcommerce.com";
        private string _browserType = "chromium";
        private bool _headless = true;
        private int _viewportWidth = 1366;
        private int _viewportHeight = 768;

        private ILocator RegisterLink => _page.Locator("a.ico-register");
        private ILocator MobileMenu => _page.Locator(".menu-toggle");
        private ILocator HeaderMenu => _page.Locator(".header-menu");

        // ========================= OneTimeSetUp =========================
        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            // ---- Read appsettings.json (اختياري) ----
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

            // ---- ENV overrides (CI-friendly) ----
            var envBrowser = Environment.GetEnvironmentVariable("BROWSER");
            if (!string.IsNullOrWhiteSpace(envBrowser)) _browserType = envBrowser;

            var envBaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
            if (!string.IsNullOrWhiteSpace(envBaseUrl)) _baseUrl = envBaseUrl;

            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            var envHeadless = Environment.GetEnvironmentVariable("HEADLESS");
            var resolvedHeadless = envHeadless switch
            {
                null when isCI => true,                     // على CI شغّل headless افتراضيًا
                null => _headless,
                _ => envHeadless.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                     envHeadless.Equals("true", StringComparison.OrdinalIgnoreCase)
            };

            // ---- Playwright startup ----
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

            // Helpful flags على Linux runners
            if (OperatingSystem.IsLinux() && (typeLower is "chromium" or "firefox"))
                launchOptions.Args = new[] { "--no-sandbox" };

            _browser = await browserType.LaunchAsync(launchOptions);
        }

        // ============================ SetUp =============================
        [SetUp]
        public async Task SetUpTest()
        {
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = _baseUrl,
                ViewportSize = new ViewportSize { Width = _viewportWidth, Height = _viewportHeight }
            });

            // ابدأ tracing لكل اختبار (يُحفظ عند الفشل فقط)
            await _context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            _page = await _context.NewPageAsync();

            // افتح الصفحة وضمن جاهزيتها (نسخة آمنة للـ CI)
            await _page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
            await EnsurePageReadyAsync();

            // التوقيتات الافتراضية (قابلة للتهيئة بالمتغير PW_TIMEOUT)
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

        // ======================= EnsurePageReady ========================
        public async Task EnsurePageReadyAsync()
        {
            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

            // تجنّب NetworkIdle في CI لأن مواقع الديمو قد لا تصل إليه
            if (isCI)
            {
                try
                {
                    await _page.WaitForLoadStateAsync(LoadState.Load, new() { Timeout = 15000 });
                }
                catch { /* تجاهل */ }
                await _page.WaitForTimeoutAsync(300);
            }
            else
            {
                try
                {
                    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 5000 });
                }
                catch { /* لا مشكلة لو لم يصل للـ idle */ }
            }

            await AcceptCookiesIfPresentAsync();
            await DismissNotificationBarIfPresentAsync();

            // عنصر جاهزية موثوق بدل .header-menu
            var readyAnchor = _page.Locator(".header-logo a, a.ico-cart").First;
            await Assertions.Expect(readyAnchor).ToBeVisibleAsync(new() { Timeout = 30000 });
        }

        private async Task AcceptCookiesIfPresentAsync()
        {
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

                await bar.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            }
        }

        private static async Task<bool> IsQuicklyVisibleAsync(ILocator locator, int timeoutMs = 800)
        {
            try
            {
                await locator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
                return true;
            }
            catch (TimeoutException) { return false; }
            catch (PlaywrightException) { return false; }
        }

        // ============================ TearDown ==========================
        [TearDown]
        public async Task AfterEach()
        {
            var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

            // paths متوافقة مع الـ workflow (يرفع PlaywrightTests/artifacts/**)
            var artifactsRoot = Path.Combine("PlaywrightTests", "artifacts");
            var screenshotsDir = Path.Combine(artifactsRoot, "screenshots");
            var tracesDir = Path.Combine(artifactsRoot, "traces");
            Directory.CreateDirectory(screenshotsDir);
            Directory.CreateDirectory(tracesDir);

            try
            {
                if (failed)
                {
                    var safeName = Sanitize(TestContext.CurrentContext.Test.Name);
                    var shotPath = Path.Combine(screenshotsDir, $"{safeName}.png");
                    var tracePath = Path.Combine(tracesDir, $"{safeName}.zip");

                    await _page.ScreenshotAsync(new() { Path = shotPath, FullPage = true });
                    TestContext.AddTestAttachment(shotPath, "Screenshot on failure");

                    await _context.Tracing.StopAsync(new() { Path = tracePath });
                    TestContext.AddTestAttachment(tracePath, "Playwright Trace on failure");
                }
                else
                {
                    await _context.Tracing.StopAsync(); // أوقف التتبع بدون حفظ
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"[TRACE/SHOT] note: {ex.Message}");
            }

            await _context.CloseAsync();
        }

        // ========================= OneTimeTearDown ======================
        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            if (_browser is not null) await _browser.CloseAsync();
            _playwright?.Dispose();

            // تنظيف أي ملفات مؤقتة باسم lastUser_*.json
            var dir = TestContext.CurrentContext.WorkDirectory;
            foreach (var file in Directory.GetFiles(dir, "lastUser_*.json"))
            {
                try { File.Delete(file); }
                catch (Exception ex) { TestContext.Out.WriteLine($"[CLEANUP-ERROR] {ex.Message}"); }
            }
        }

        // ============================== Utils ===========================
        protected async Task PrintBrowserInfoAsync()
        {
            var ua = await _page.EvaluateAsync<string>("navigator.userAgent");
            TestContext.Out.WriteLine($"[INFO] UA: {ua}");
        }

        private static string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
