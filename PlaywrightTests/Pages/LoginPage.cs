using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}

