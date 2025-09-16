using Microsoft.Playwright;
using System.Buffers.Text;
using System.Threading.Tasks;
using static Microsoft.Playwright.Assertions;

namespace PlaywrightTests.Pages
{
    public class HomePage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;
        public HomePage(IPage page, string baseUrl)
        {
            _page = page;
            _baseUrl = baseUrl;
        }

        // Header locators
        //public ILocator RegisterLink => _page.Locator("a.ico-register");
        public ILocator LoginLink => _page.Locator("a.ico-login");
        public ILocator SearchInput => _page.Locator("#small-searchterms");
        public ILocator SearchButton => _page.Locator("button.search-box-button");
        public ILocator CartLink => _page.Locator("a.ico-cart");
        public ILocator RegisterLink => _page.Locator("a.ico-register, .header-links a[href*='register']");
        public ILocator MobileMenu => _page.Locator(".menu-toggle, .mobile-menu-toggle, .responsive-nav-button");


        // In your HomePage.cs file

        public async Task GoToAsync(string baseUrl)
        {
            // 1. Navigate to the page with a reliable load strategy and long timeout.
            await _page.GotoAsync(baseUrl!, new()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 60_000
            });

            // 2. Defensively handle the cookie banner using your robust locator.
            var cookieOk = _page.Locator("#eu-cookie-ok, .eu-cookie-bar-notification .close, .eu-cookie-bar button");
            try
            {
                // Directly try to click the button, waiting up to 2 seconds.
                await cookieOk.ClickAsync(new() { Timeout = 2000 });
            }
            catch (TimeoutException)
            {
                // If the button isn't clickable within 2s, simply continue.
            }

            // 3. Cleverly handle mobile vs. desktop viewports.
            try
            {
                // First, check for the link directly (for desktop view).
                await Assertions.Expect(RegisterLink).ToBeVisibleAsync(new() { Timeout = 1000 });
            }
            catch (PlaywrightException)
            {
                // If it fails, assume it's a mobile view and click the hamburger menu.
                if (await MobileMenu.IsVisibleAsync())
                {
                    await MobileMenu.ClickAsync();
                }
            }

            // 4. Perform the final, definitive assertion to ensure the page is ready.
            await Assertions.Expect(RegisterLink).ToBeVisibleAsync();
        }
        public async Task OpenLoginAsync()
        {
            await Expect(LoginLink).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await LoginLink.ClickAsync();
        }
        public async Task OpenRegisterAsync()
        {
                await Expect(RegisterLink).ToBeVisibleAsync(new() { Timeout = 10_000 });
                await RegisterLink.ClickAsync();
        }


        /// <summary>Opens the Cart from the header.</summary>
        public async Task OpenCartAsync()
        {
            await GoToAsync(_baseUrl);
            await Expect(CartLink).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await CartLink.ClickAsync();
        }

        /// <summary>Performs a search via the header search box.</summary>
        public async Task<SearchResultsPage> SearchAsync(string query)
        {
            await _page.GotoAsync(_baseUrl); // لا networkidle
            var box = _page.Locator("#small-searchterms");
            await Microsoft.Playwright.Assertions.Expect(box).ToBeVisibleAsync(new() { Timeout = 10000 });
            await box.FillAsync(query);
            await box.PressAsync("Enter");
            return new SearchResultsPage(_page);

            // If you later need a page object return:
            // public async Task<SearchResultsPage> SearchAsync(string query)
            // {
            //     await SearchAsync(query);
            //     return new SearchResultsPage(_page);
            // }
        }
    }
}
