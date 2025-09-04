using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightTests.Pages;   
using PlaywrightTests.Utils;   
using System.Threading.Tasks;

namespace PlaywrightTests.Tests.Auth
{
    public class RegisterTests : TestBase
    {
        [Test]
        public async Task Register_HappyPath_ShowsSuccessAndLogsUserIn()
        {

        // Arrange
        var home = new HomePage(_page);
        await home.GoToAsync(_baseUrl);
        // Print browser info
        await PrintBrowserInfoAsync();

        // Act – open Register and submit valid data
        await home.OpenRegisterAsync();
        var register =new RegisterPage(_page);

        var email = Faker.RandomEmail();
        const string firstName = "Heba";
        const string lastName = "QA";
        const string password = "StrongPass!123";

         await  register.RegisterAsync(firstName, lastName, email, password);

        // Assert – success message + user is logged in (My account visible)
        await Assertions.Expect(register.SuccessMessage).ToContainTextAsync("Your registration completed");
        await register.ContinueAsync();
        await Assertions.Expect(_page.Locator("a.ico-account")).ToBeVisibleAsync();


        }
    }
}
