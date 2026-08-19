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
        public ILocator RegisterLink => _page.Locator("a.ico-register");
        public ILocator LoginLink => _page.Locator("a.ico-login");
        public ILocator SearchInput => _page.Locator("#small-searchterms");
        public ILocator SearchButton => _page.Locator("button.search-box-button");
        public ILocator CartLink => _page.Locator("a.ico-cart");
        private ILocator MobileMenu => _page.Locator(".menu-toggle");
        public ILocator WishlistLink => _page.Locator("a.ico-wishlist");

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
            //await GoToAsync(_baseUrl);
            await Expect(CartLink).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await CartLink.ClickAsync();
        }

        /// <summary>Performs a search via the header search box.</summary>
        public async Task<SearchResultsPage> SearchAsync(string query)
        {
            //await _page.GotoAsync(_baseUrl); // لا networkidle
            var box = _page.Locator("#small-searchterms");
            await Microsoft.Playwright.Assertions.Expect(box).ToBeVisibleAsync(new() { Timeout = 10000 });
            await box.FillAsync(query);
            await box.PressAsync("Enter");
            return new SearchResultsPage(_page);

        }
        public async Task OpenWishlistAsync()
        {
            await Expect(WishlistLink).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await WishlistLink.ClickAsync();
        }
    }
}
