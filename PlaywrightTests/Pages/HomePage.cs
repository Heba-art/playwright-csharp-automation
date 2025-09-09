using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class HomePage
    {
        private readonly IPage _page;
        public HomePage (IPage page) => _page = page;
        private ILocator RegisterLink => _page.Locator("a.ico-register");
        private ILocator LoginLink => _page.Locator("a.ico-login");
        private ILocator SearchInput => _page.Locator("#small-searchterms");
        private ILocator SearchButton => _page.Locator("button.search-box-button");
        public async Task GoToAsync(string baseUrl) => await _page.GotoAsync(baseUrl);
        public async Task OpenRegisterAsync() => await RegisterLink.ClickAsync();
        public async Task OpenLoginAsync() => await LoginLink.ClickAsync();

        //Performs a search from the header and returns the search results page object
        public async Task<SearchResultsPage> SearchAsync(string query)
        {
            await SearchInput.FillAsync(query);
            await SearchButton.ClickAsync();
            return new SearchResultsPage(_page);
        }

    }
}
