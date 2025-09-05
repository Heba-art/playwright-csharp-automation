using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlaywrightTests.Utils
{
    public class TestBase
    {
        protected IPlaywright _playwright = default!;
        protected IBrowser _browser = default!;
        protected IBrowserContext _context = default!;
        protected IPage _page = default!;
        protected string _baseUrl = "https://demo.nopcommerce.com";

        private string _browserType = "chromium";
        private bool _headless = false;
        private int _viewportWidth = 1366;
        private int _viewportHeight = 768;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            // Load configuration from appsettings.json if available
            if (File.Exists("appsettings.json")){
            var json = await File.ReadAllTextAsync("appsettings.json");
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            _baseUrl = root.GetProperty("baseUrl").GetString()!;
            _browserType = root.GetProperty("browser").GetString()!;
            _headless = root.GetProperty("headless").GetBoolean();
            _viewportWidth = root.GetProperty("viewport").GetProperty("width").GetInt32();
            _viewportHeight = root.GetProperty("viewport").GetProperty("height").GetInt32();
            }
            _playwright = await Playwright.CreateAsync();
            // Select browser type from config
            IBrowserType browserType = _browserType.ToLower() switch
            {
                "firefox" => _playwright.Firefox,
                "webkit" => _playwright.Webkit,
                _ => _playwright.Chromium
            };
            // Launch the browser with configured options
            _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
                {
                Headless = _headless
            });  

        }

        [SetUp]
        public async Task setup()
        {
         // Create a new browser context for each test
         _context = await _browser.NewContextAsync(new BrowserNewContextOptions
         {
             ViewportSize = new ViewportSize
             {
                 Width = _viewportWidth,
                 Height = _viewportHeight
             }
         });
            // Create a new page (tab) inside the context
            _page = await _context.NewPageAsync(); 

        }

        [TearDown]
        public async Task TearDown()
        {
            // Close the context after each test
            await _context.CloseAsync();
        }
        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            // Close browser and dispose Playwright engine after all tests
            await _browser.CloseAsync();
            _playwright.Dispose();

        // Clean up credential files
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

        // Optional: quick browser info dump
        protected async Task PrintBrowserInfoAsync()
        {
            var ua = await _page.EvaluateAsync<string>("navigator.userAgent");
            TestContext.Out.WriteLine($"[INFO] UA: {ua}");
        }
    }

}





