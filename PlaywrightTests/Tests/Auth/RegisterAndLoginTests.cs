using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightTests.Pages;
using PlaywrightTests.Utils;

namespace PlaywrightTests.Tests.Auth
{
    [TestFixture]
    public class RegisterAndLoginTests : TestBase
    {
        /// <summary>
        /// This runs ONCE before any tests in this class.
        /// Its job is to ensure a user exists for the login tests to use.
        /// It creates a user ONLY if one doesn't already exist from a previous run.
        ///// </summary>
        //[OneTimeSetUp]
        //public async Task FixtureSetup()
        //{
        //    // Try to load credentials. If they exist, we don't need to create a new user.
        //    var creds = await CredentialStore.LoadAsync();
        //    if (creds != null)
        //    {
        //        TestContext.Out.WriteLine("✅ User credentials already exist. Skipping new user creation.");
        //        return;
        //    }

        //    TestContext.Out.WriteLine("🔧 No existing credentials found. Creating a new user for this session...");
        //    // Create a temporary, isolated browser context to register the user.
        //    // This prevents interference with the contexts created for each test.
        //    await using var tempContext = await _browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = _baseUrl });
        //    var tempPage = await tempContext.NewPageAsync();

        //    var home = new HomePage(tempPage, _baseUrl);
        //    await home.OpenRegisterAsync();

        //    var email = Faker.RandomEmail();
        //    var pass = "StrongPass!12345"; // Use a strong, fixed password

        //    var registerPage = new RegisterPage(tempPage);
        //    await registerPage.RegisterAsync("Fixture", "User", email, pass);

        //    // IMPORTANT: This user is saved for the login tests below.
        //    await CredentialStore.SaveAsync(email, pass);
        //    TestContext.Out.WriteLine($"✅ Successfully created and saved user: {email}");
        //}

        /// <summary>
        /// This test verifies the UI and flow of the registration process.
        /// It creates its own temporary user and does NOT save the credentials for other tests.
        /// Its purpose is only to confirm that the registration page works as expected.
        /// </summary>
        [Test, Order(1)]
        public async Task Register_HappyPath_ShowsSuccessAndLogsUserIn()
        {
            // Arrange
            var home = new HomePage(_page, _baseUrl);
            await home.OpenRegisterAsync();

            var registerPage = new RegisterPage(_page);
            var email = Faker.RandomEmail();
            const string password = "StrongPass!123";

            // Act – Register a new, temporary user for this test only.
            await registerPage.RegisterAsync("Heba", "QA", email, password);

            // Assert – Ensure the success message appears and the user is logged in.
            await Assertions.Expect(registerPage.SuccessMessage).ToContainTextAsync("Your registration completed");
            await CredentialStore.SaveAsync(email, password);
            await registerPage.ContinueAsync();
            await Assertions.Expect(_page.Locator("a.ico-account")).ToBeVisibleAsync();

            TestContext.Out.WriteLine($"✅ UI test passed: Logged in as temporary user: {email}");
            // CHANGE: We no longer save credentials here. This test is self-contained.
        }

        /// <summary>
        /// This test verifies that the user created in OneTimeSetUp can log in.
        /// </summary>
        [Test, Order(2)]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange - Load the user created in our FixtureSetup.
            var creds = await CredentialStore.LoadAsync();
            Assert.That(creds, Is.Not.Null, "Credentials could not be loaded. The OneTimeSetUp might have failed.");

            var home = new HomePage(_page, _baseUrl);
            await home.OpenLoginAsync();

            // Act
            var loginPage = new LoginPage(_page);
            await loginPage.LoginAsync(creds!.Email, creds.Password);

            // Assert
            await Assertions.Expect(_page.Locator("a.ico-account")).ToBeVisibleAsync();
            TestContext.Out.WriteLine($"✅ Login succeeded for user: {creds.Email}");
        }

        /// <summary>
        /// This test verifies that login fails when using an incorrect password.
        /// </summary>
        [Test, Order(3)]
        public async Task Login_WithInvalidPassword_ShouldShowError_AndNotLogin()
        {
            // Arrange – Load the user and create a wrong password.
            var creds = await CredentialStore.LoadAsync();
            Assert.That(creds, Is.Not.Null, "Credentials could not be loaded. The OneTimeSetUp might have failed.");
            var wrongPassword = creds!.Password + "XYZ";

            var home = new HomePage(_page, _baseUrl);
            await home.OpenLoginAsync();

            var loginPage = new LoginPage(_page);

            // Act – Try to log in with the wrong password.
            await loginPage.LoginAsync(creds.Email, wrongPassword);

            // Assert – Ensure an error message appears and the user is not logged in.
            await Assertions.Expect(loginPage.ErrorSummaryLocator).ToBeVisibleAsync();
            await Assertions.Expect(loginPage.ErrorSummaryLocator).ToContainTextAsync("Login was unsuccessful");
            await Assertions.Expect(_page.Locator("a.ico-account")).Not.ToBeVisibleAsync();

            TestContext.Out.WriteLine("✅ Negative login check passed (error shown, no login).");
        }
    }
}