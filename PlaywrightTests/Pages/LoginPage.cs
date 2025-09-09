using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class LoginPage
    {

    private readonly IPage _page;

     public LoginPage(IPage page) => _page = page;

    private ILocator EmailInput => _page.Locator("#Email");
    private ILocator PasswordInput => _page.Locator("#Password");
    private ILocator LoginButton => _page.Locator("button.login-button");
    public ILocator MyAccountLink => _page.Locator("a.ico-account");
    private ILocator ErrorSummary => _page.Locator(".message-error.validation-summary-errors");


        public async Task LoginAsync (string email, string password)
    {
            await EmailInput.FillAsync(email);
            await PasswordInput.FillAsync(password);
            await LoginButton.ClickAsync(); 
     }
    public async Task<bool> IsMyAccountVisibleAsync()
    {
        return await MyAccountLink.IsVisibleAsync();
    }
    public async Task<bool> IsErrorVisibleAsync()
        => await ErrorSummary.IsVisibleAsync();

    public ILocator ErrorSummaryLocator => ErrorSummary;

    public async Task<string> GetErrorTextAsync()
        => (await ErrorSummary.InnerTextAsync() ?? string.Empty).Trim();

    }
}

