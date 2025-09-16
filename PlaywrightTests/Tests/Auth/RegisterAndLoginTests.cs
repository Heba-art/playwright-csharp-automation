using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using PlaywrightTests.Pages;   
using PlaywrightTests.Utils; 


namespace PlaywrightTests.Tests.Auth
{
    [TestFixture]
    public class RegisterAndLoginTests : TestBase
    {
        [OneTimeSetUp]
        public async Task EnsureUserExists()
        {
            // Try to load last saved credentials; if present, we're done
            var creds = CredentialStore.LoadAsync();
            if (creds != null) return;

            // Create a temporary context so we don't interfere with per-test contexts
            var tempContext = await _browser.NewContextAsync(
                new BrowserNewContextOptions { BaseURL = _baseUrl }
            );

            try
            {
                var tempPage = await tempContext.NewPageAsync();

                var home = new HomePage(tempPage, _baseUrl);
                await home.OpenRegisterAsync();

                // Use your static Faker helper
                var email = Faker.RandomEmail();
                var pass = $"Qa!{DateTime.UtcNow:yyyyMMddHHmmss}a1";

                var register = new RegisterPage(tempPage);
                // Use the method you already use in TC1
                await register.RegisterAsync(first: "Heba", last: "QA", email: email, pass: pass);

                // Save credentials for login tests
                await CredentialStore.SaveAsync(email, pass);
            }
            finally
            {
                await tempContext.CloseAsync();
            }
        }

        [Test, Order(1)]
        public async Task Register_HappyPath_ShowsSuccessAndLogsUserIn()
        {

            // Arrange
            var home = new HomePage(_page, _baseUrl);
            await home.GoToAsync(_baseUrl);
            // Print browser info
            await PrintBrowserInfoAsync();

            // Act – open Register and submit valid data
            await home.OpenRegisterAsync();
            var register = new RegisterPage(_page);

            var email = Faker.RandomEmail();
            const string firstName = "Heba";
            const string lastName = "QA";
            const string password = "StrongPass!123";

            await register.RegisterAsync(firstName, lastName, email, password);

            // Assert – success message + user is logged in (My account visible)
            await Assertions.Expect(register.SuccessMessage).ToContainTextAsync("Your registration completed");
            // Save creds for TC2
            await CredentialStore.SaveAsync(email, password);

            await register.ContinueAsync();
            await Assertions.Expect(_page.Locator("a.ico-account")).ToBeVisibleAsync();

            TestContext.Out.WriteLine($"✅ Registered & logged in as: {email}");

        }
        [Test, Order(2)]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Load the latest registered user from TC001
            var creds = await CredentialStore.LoadAsync();
            Assert.That(creds, Is.Not.Null, "No saved credentials found. Run TC-001 (Register) first.");

            var home = new HomePage(_page, _baseUrl);
            await home.GoToAsync(_baseUrl);
            await home.OpenLoginAsync();

            var loginPage = new LoginPage(_page);
            await loginPage.LoginAsync(creds!.Email, creds.Password);

            Assert.That(await loginPage.IsMyAccountVisibleAsync(),
                    Is.True, "Login failed: 'My account' link not visible");

            TestContext.Out.WriteLine($"✅ Login succeeded for: {creds.Email}");


        }
        [Test, Order(3)]
        public async Task Login_WithInvalidPassword_ShouldShowError_AndNotLogin()
        {
        // Arrange – Read data of an existing user (from TC-001)
        var creds = await CredentialStore.LoadAsync();
        Assert.That(creds,Is.Not.Null,"No saved credentials found. Run TC-001 (Register) first.");
        //Intentionally wrong password
        var wrongPassword = creds!.Password + "XYZ";

        var home = new HomePage(_page,_baseUrl);
        await home.GoToAsync(_baseUrl);
        await home.OpenLoginAsync();

        var loginPage = new LoginPage(_page);
        // Act – Try to log in with the wrong password
        await loginPage.LoginAsync(creds.Email,wrongPassword);

        // Assert – An error megs appears, and “My account” does not appear.
        Assert.That(await loginPage.IsErrorVisibleAsync(),Is.True, "Expected login error summary to be visible, but it wasn't.");
        
        var errorText = await loginPage.GetErrorTextAsync();
        TestContext.Out.WriteLine($"🔎 Actual error text from site: '{errorText}'");
        Assert.That(errorText, Does.Contain("Login was unsuccessful"), "Expected unsuccessful login message.");
        
        Assert.That(await loginPage.IsMyAccountVisibleAsync(), Is.False, "User should not be logged in when using a wrong password.");
        TestContext.Out.WriteLine("✅ Negative login check passed (error shown, no login).");



        }
    }
}
