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

        // In your HomePage.cs file
        // In your HomePage.cs file

        public async Task GoToAsync(string baseUrl)
        {
            // 1. Navigate to the page and handle cookies (unchanged).
            await _page.GotoAsync(baseUrl!, new()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 60_000
            });

            var cookieOk = _page.Locator("#eu-cookie-ok, .eu-cookie-bar-notification .close, .eu-cookie-bar button");
            try
            {
                await cookieOk.ClickAsync(new() { Timeout = 2000 });
            }
            catch (TimeoutException)
            {
                // Ignore if the button doesn't appear.
            }

            // 2. New, simplified logic to ensure the link is visible.
            // First, quietly check if the link is already visible (takes 1s max).
            var isLinkVisible = await RegisterLink.IsVisibleAsync(new() { Timeout = 1000 });

            // If it's not visible, we're likely in mobile view.
            if (!isLinkVisible && await MobileMenu.IsVisibleAsync())
            {
                // Click the menu to reveal the links.
                await MobileMenu.ClickAsync();
            }

            // 3. The final and only assertion.
            // Give it a long timeout (15 seconds) to be robust against any network slowness or page animations.
            await Assertions.Expect(RegisterLink).ToBeVisibleAsync(new() { Timeout = 15_000 });
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

        }
    }
}
