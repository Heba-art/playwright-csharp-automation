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
        public async Task GoToAsync(string baseUrl) => await _page.GotoAsync(baseUrl);
        public async Task OpenRegisterAsync() => await RegisterLink.ClickAsync();
        public async Task OpenLoginAsync() => await LoginLink.ClickAsync();

    }
}
