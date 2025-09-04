using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class RegisterPage
    {
        private readonly IPage _page;
        public RegisterPage(IPage page) => _page = page;

        private ILocator GenderFemale => _page.Locator("#gender-female");
        private ILocator FirstName => _page.Locator("#FirstName");
        private ILocator LastName => _page.Locator("#LastName");
        private ILocator Email => _page.Locator("#Email");
        private ILocator Password => _page.Locator("#Password");
        private ILocator Confirm => _page.Locator("#ConfirmPassword");
        private ILocator RegisterBtn => _page.Locator("#register-button");
        private ILocator SuccessMsg => _page.Locator(".result");

        public async Task RegisterAsync(string first, string last, string email, string pass)
        {
            await GenderFemale.CheckAsync();
            await FirstName.FillAsync(first);
            await LastName.FillAsync(last);
            await Email.FillAsync(email);
            await Password.FillAsync(pass);
            await Confirm.FillAsync(pass);
            await RegisterBtn.ClickAsync();

        }
        public ILocator SuccessMessage => SuccessMsg;
        public async Task ContinueAsync() => await _page.ClickAsync("a.register-continue-button");

    }
}
