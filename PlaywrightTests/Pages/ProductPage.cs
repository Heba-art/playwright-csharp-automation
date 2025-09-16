using Microsoft.Playwright;
using System.Globalization;

namespace PlaywrightTests.Pages
{
    public class ProductPage
    {
        private readonly IPage _page;
        public ProductPage(IPage page) => _page = page;

        public ILocator ProductTitle => _page.Locator("div.product-name h1");
        public ILocator SuccessBar => _page.Locator("#bar-notification");
        private ILocator AddToCartButton => _page.Locator("button#add-to-cart-button-1");
        private ILocator CartBadge => _page.Locator("span.cart-qty");
        private ILocator PriceOnPdp => _page.Locator("span.price-value-1");

        public async Task<string> GetDisplayedPriceAsync() => (await PriceOnPdp.InnerTextAsync()).Trim();
        public async Task SelectProcessorAsync(string text)
            => await _page.Locator("#product_attribute_1").SelectOptionAsync(new SelectOptionValue { Label = text });

        public async Task SelectRamAsync(string text)
            => await _page.Locator("#product_attribute_2").SelectOptionAsync(new SelectOptionValue { Label = text });

        public async Task SelectHddAsync(string labelText)
            => await _page.GetByLabel(labelText).CheckAsync(); // radios

        public async Task SelectOsAsync(string labelText)
            => await _page.GetByLabel(labelText).CheckAsync(); // radios

        public async Task UncheckSoftwareOptionsAsync()
        {
            // software checkboxes may inflate the price; ensure all are unchecked
            var software = _page.Locator("input[name='product_attribute_5']");
            var count = await software.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var cb = software.Nth(i);
                if (await cb.IsCheckedAsync()) await cb.UncheckAsync();
            }
        }
        public async Task ApplyBaseConfigurationAsync()
        {
            await _page.Locator("#product_attribute_1")
        .SelectOptionAsync(new SelectOptionValue { Label = "2.2 GHz Intel Pentium Dual-Core E2200" });

            await _page.Locator("#product_attribute_2")
                .SelectOptionAsync(new SelectOptionValue { Label = "2 GB" });

            await _page.GetByLabel("320 GB").CheckAsync();
            await _page.GetByLabel("Vista Home").CheckAsync();

            // Uninstall any additional programs.
            var software = _page.Locator("input[name='product_attribute_5']");
            for (int i = 0; i < await software.CountAsync(); i++)
                if (await software.Nth(i).IsCheckedAsync()) await software.Nth(i).UncheckAsync();

            // Wait for the price to "stabilize" and preferably reach 1250
            await WaitForPriceAsync(expected: 1250m, timeoutMs: 15000);
        }

        public async Task AddToCartAsync()
        {
            // Ensure badge baseline
            var before = (await CartBadge.InnerTextAsync()).Trim();
            
            await Microsoft.Playwright.Assertions.Expect(AddToCartButton).ToBeEnabledAsync();
            await AddToCartButton.ClickAsync();

            var toastTask = _page.Locator("#bar-notification.success").WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 8000
            });

            var badgeChangeTask = _page.WaitForFunctionAsync(
                @"(sel, prev) => {
                    const el = document.querySelector(sel);
                    return !!el && el.textContent.trim() !== prev;
                }",
                new[] { "span.cart-qty", before },
                new() { Timeout = 8000 });

            var winner = await Task.WhenAny(toastTask, badgeChangeTask);
            if (winner == toastTask)
            {
                var bar = _page.Locator("#bar-notification.success");
                try
                {
                    await bar.Locator(".close").ClickAsync();
                    await Microsoft.Playwright.Assertions.Expect(bar).ToBeHiddenAsync();
                }
                catch { /* ignore */ }
            }
            else
            {
                TestContext.Out.WriteLine("✅ Cart badge changed, item added.");
            }
        }
        public async Task WaitForPriceAsync(decimal? expected = null, int timeoutMs = 15000, int stableWindowMs = 800)
        {
            var priceEl = _page.Locator("span.price-value-1"); // selector for the price element
            var sw = System.Diagnostics.Stopwatch.StartNew();

            string? lastText = null; // last observed price text
            int stableMs = 0;        // how long the price has remained unchanged

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var raw = (await priceEl.InnerTextAsync()).Trim();

                // If an expected value is provided, check whether the price matches it
                if (expected.HasValue)
                {
                    decimal current = decimal.Parse(
                        System.Text.RegularExpressions.Regex.Replace(raw, @"[^\d\.]", ""), // remove all non-numeric and non-dot characters
                        System.Globalization.CultureInfo.InvariantCulture);

                    if (Math.Abs(current - expected.Value) < 0.01m)
                        return; // price reached the target (within 0.01 tolerance)
                }

                // Check if the text is stable (not changing between polls)
                if (raw == lastText)
                {
                    await _page.WaitForTimeoutAsync(200); // wait before checking again
                    stableMs += 200;
                    if (stableMs >= stableWindowMs)
                        return; // price has stayed the same long enough -> considered stable
                }
                else
                {
                    // If the price text changed, reset stability counter
                    lastText = raw;
                    stableMs = 0;
                    await _page.WaitForTimeoutAsync(200);
                }
            }

            // If timeout expires, throw an error with the last seen price
            var currentText = await priceEl.InnerTextAsync();
            throw new TimeoutException($"Price did not stabilize/reach expected value within {timeoutMs}ms. Current price: '{currentText}'.");
        }
    }
}