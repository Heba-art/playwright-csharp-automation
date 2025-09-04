using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightTests.Pages;   // صفحات الـ POM
using PlaywrightTests.Utils;   // TestBase + Faker
using System.Threading.Tasks;

namespace PlaywrightTests.Tests.Auth
{
    // لازم تكون public وترث من TestBase عشان _page و _baseUrl
    public class RegisterAndLoginTests : TestBase
    {
        [Test]
        public async Task UserCanRegisterThenLogin()
        {
            var home = new HomePage(_page);
            await home.GoToAsync(_baseUrl);

            // Register
            await home.OpenRegisterAsync();
            var register = new RegisterPage(_page);
            var email = Faker.RandomEmail();
            const string password = "StrongPass!123";

            await register.RegisterAsync("Heba", "QA", email, password);

            // Assertion على رسالة النجاح
            await Microsoft.Playwright.Assertions
                .Expect(register.SuccessMessage)
                .ToContainTextAsync("Your registration completed");

            await register.ContinueAsync();

            // Logout
            await _page.ClickAsync("a.ico-logout");

            // Login
            await home.OpenLoginAsync();
            var login = new LoginPage(_page);
            await login.LoginAsync(email, password);

            // Assert: يظهر My account بعد تسجيل الدخول
            await Microsoft.Playwright.Assertions
                .Expect(_page.Locator("a.ico-account"))
                .ToBeVisibleAsync();
        }
    }
}
