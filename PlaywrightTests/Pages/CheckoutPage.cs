using Microsoft.Playwright;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class CheckoutPage
    {
        private readonly IPage _page;

        public CheckoutPage(IPage page) => _page = page;

        // Guest checkout
        public ILocator CheckoutAsGuestButton =>
            _page.GetByRole(AriaRole.Button,
                new() { Name = "Checkout as Guest", Exact = true });

        // Billing fields
        public ILocator FirstName =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "First name:" });

        public ILocator LastName =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "Last name:" });

        public ILocator Email =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "Email:" });

        public ILocator Country =>
            _page.GetByLabel("Country:");

        public ILocator State =>
            _page.GetByLabel("State / province:");

        public ILocator City =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "City:" });

        public ILocator Address1 =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "Address 1:" });

        public ILocator ZipCode =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "Zip / postal code:" });

        public ILocator PhoneNumber =>
            _page.GetByRole(AriaRole.Textbox, new() { Name = "Phone number:" });
        public ILocator BillingSection =>
           _page.Locator("#opc-billing");

        public ILocator BillingContinueButton =>
            BillingSection.GetByRole(
                AriaRole.Button,
                new() { Name = "Continue", Exact = true });
        public ILocator ShippingMethodSection =>
             _page.Locator("#opc-shipping_method");

        public ILocator GroundShippingOption =>
            ShippingMethodSection.GetByRole(
                AriaRole.Radio,
                new() { Name = "Ground ($0.00)" });

        public ILocator ShippingContinueButton =>
            ShippingMethodSection.GetByRole(
                AriaRole.Button,
                new() { Name = "Continue", Exact = true });

        public ILocator PaymentMethodSection =>
            _page.Locator("#opc-payment_method");

        public ILocator CreditCardOption =>
            PaymentMethodSection.GetByRole(
                AriaRole.Radio,
                new()
                {
                    NameRegex = new System.Text.RegularExpressions.Regex("Credit Card")
                });

        public ILocator PaymentMethodContinueButton =>
            PaymentMethodSection.GetByRole(
                AriaRole.Button,
                new() { Name = "Continue", Exact = true });
        public ILocator PaymentInfoSection =>
    _page.Locator("#opc-payment_info");

        public ILocator CardholderName =>
            PaymentInfoSection.GetByLabel("Cardholder name:");

        public ILocator CardNumber =>
            PaymentInfoSection.GetByLabel("Card number:");

        public ILocator ExpireMonth =>
            PaymentInfoSection.Locator("#ExpireMonth");

        public ILocator ExpireYear =>
            PaymentInfoSection.Locator("#ExpireYear");

        public ILocator CardCode =>
            PaymentInfoSection.GetByLabel("Card code:");

        public ILocator PaymentInfoContinueButton =>
            PaymentInfoSection.GetByRole(
                AriaRole.Button,
                new() { Name = "Continue", Exact = true });

        public ILocator ConfirmOrderSection =>
            _page.Locator("#opc-confirm_order");

        public ILocator ConfirmButton =>
            ConfirmOrderSection.GetByRole(
                AriaRole.Button,
                new() { Name = "Confirm", Exact = true });

        public ILocator OrderSuccessHeading =>
            _page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Your order has been successfully processed!" });

        public async Task ChooseGuestCheckoutAsync()
        {
            await CheckoutAsGuestButton.ClickAsync();
        }
        public async Task FillBillingAddressAsync()
        {
            await FirstName.FillAsync("Taylor");
            await LastName.FillAsync("Morgan");
            await Email.FillAsync("taylor.morgan@example.com");

            await Country.SelectOptionAsync(
                new SelectOptionValue { Label = "United States of America" });

            await State.SelectOptionAsync(
                new SelectOptionValue { Label = "California" });

            await City.FillAsync("Los Angeles");
            await Address1.FillAsync("123 Main Street");
            await ZipCode.FillAsync("90001");
            await PhoneNumber.FillAsync("0412345678");

            await BillingContinueButton.ClickAsync();
        }
        public async Task ChooseShippingMethodAsync()
        {
            await GroundShippingOption.CheckAsync();
            await ShippingContinueButton.ClickAsync();
        }

        public async Task ChoosePaymentMethodAsync()
        {
            await CreditCardOption.CheckAsync();
            await PaymentMethodContinueButton.ClickAsync();
        }
        public async Task FillPaymentInformationAsync()
        {
            await CardholderName.FillAsync("Taylor Morgan");
            await CardNumber.FillAsync("4111111111111111");

            await ExpireMonth.SelectOptionAsync("12");
            await ExpireYear.SelectOptionAsync("2030");

            await CardCode.FillAsync("123");

            await PaymentInfoContinueButton.ClickAsync();
        }

        public async Task ConfirmOrderAsync()
        {
            await ConfirmButton.ClickAsync();
        }

        public async Task WaitForOrderSuccessAsync()
        {
            await Microsoft.Playwright.Assertions.Expect(OrderSuccessHeading)
                .ToBeVisibleAsync(new() { Timeout = 15000 });
        }
    }
}