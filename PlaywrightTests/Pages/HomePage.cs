using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightTests.Pages
{
    public class HomePage
    {
        private readonly IPage _page;

        public HomePage(IPage page) => _page = page;

        public ILocator RegisterLink => _page.Locator("a.ico-register");
        public ILocator LoginLink => _page.Locator("a.ico-login");
        public ILocator SearchInput => _page.Locator("#small-searchterms");
        public ILocator SearchButton => _page.Locator("button.search-box-button");
        // المحدد المصحح
        public ILocator CartLink => _page.Locator("a.ico-cart");

        public async Task GoToAsync(string baseUrl) => await _page.GotoAsync(baseUrl);

        public async Task OpenRegisterAsync() => await RegisterLink.ClickAsync();

        public async Task OpenLoginAsync() => await LoginLink.ClickAsync();

        public async Task OpenCartAsync()
        {
            var cartLink = _page.Locator("a.ico-cart");
            await cartLink.ClickAsync();
        }


        //Performs a search from the header and returns the search results page object
        public async Task<SearchResultsPage> SearchAsync(string query)
        {
            await SearchInput.FillAsync(query);
            await SearchButton.ClickAsync();
            return new SearchResultsPage(_page);
        }


    }
}
