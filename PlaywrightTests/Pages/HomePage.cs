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


        public async Task GoToAsync(string baseUrl)
        {
            await _page.GotoAsync(baseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

            // شريط الكوكيز (إن وجد): انتظر 1s ثم اضغط، بدون تحذير Obsolete
            var cookieOk = _page.Locator("#eu-cookie-ok, .eu-cookie-bar-notification .close, .eu-cookie-bar button");
            try
            {
                await cookieOk.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
                await cookieOk.ClickAsync();
            }
            catch (TimeoutException)
            {
                // لم يظهر زر الكوكيز خلال ثانية - تجاهل
            }

            // افتح قائمة الموبايل إذا لم يظهر زر Register خلال 1s
            try
            {
                await Assertions.Expect(RegisterLink).ToBeVisibleAsync(new() { Timeout = 1000 });
            }
            catch (PlaywrightException)
            {
                if (await MobileMenu.IsVisibleAsync())
                    await MobileMenu.ClickAsync();
            }

            // التأكيد النهائي يعتمد على DefaultTimeout المضبوط في TestBase
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
