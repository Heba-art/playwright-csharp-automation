using NUnit.Framework;
using PlaywrightTests.Pages;
using PlaywrightTests.Utils;
using System.Threading.Tasks;

namespace PlaywrightTests.Tests.Catalog
{
    [TestFixture]
    public class CartTests : TestBase
    {
        [Test, Order(5)]
        public async Task AddToCart_FromProductPage_ShouldShowInCart_WithCorrectNameAndPrice()
        {
            const string product = "Build your own computer";

            // Go to home and search exact match
            var home = new HomePage(_page);
            await home.GoToAsync(_baseUrl);

            var resultsPage = await home.SearchAsync(product);

            // Open the product by its title link
            var productLink = resultsPage
                .ProductTitles
                .Filter(new() { HasText = product })
                .First;
            await productLink.ClickAsync();

            // Act – configure + wait price to settle, then add to cart
            var productPage = new ProductPage(_page);

            // 1) Apply default settings (CPU/RAM/HDD/OS… depending on page)
            await productPage.ApplyBaseConfigurationAsync();

            // 2) Wait for the price to stabilize/reach the expected value (1250) before reading/adding.
            await productPage.WaitForPriceAsync(expected: 1250m, timeoutMs: 15000);

            var pdpPriceRaw = (await productPage.GetDisplayedPriceAsync())?.Trim();

            await Microsoft.Playwright.Assertions.Expect(productPage.ProductTitle)
                .ToHaveTextAsync(product);

            await productPage.AddToCartAsync();

            // Go to cart
            await home.OpenCartAsync();
            var cart = new CartPage(_page);
            await cart.WaitForLoadedAsync();

            // Assert - Confirm that the card is not empty
            Assert.That(await cart.IsCartEmpty(), Is.False,
                "The cart page is empty, which means the product was not added successfully.");

            // Assert – item present with correct name, qty, and price
            var name = await cart.GetFirstItemNameAsync();
            var unitPriceRaw = (await cart.GetFirstItemUnitPriceRawAsync())?.Trim();
            var qty = await cart.GetFirstItemQtyAsync();

            Assert.That(name, Does.Contain(product), "Wrong product name in cart.");
            Assert.That(qty, Is.EqualTo(1), "Quantity should be 1 after first add to cart.");

            // Parse both prices as decimals and compare numerically
            var pdpPrice = CartPage.ParsePrice(pdpPriceRaw ?? string.Empty);
            var unitPrice = CartPage.ParsePrice(unitPriceRaw ?? string.Empty);

            Assert.That(unitPrice, Is.EqualTo(pdpPrice),
                $"Unit price mismatch. PDP: {pdpPrice:C2}, Cart: {unitPrice:C2} (raw '{pdpPriceRaw}' vs '{unitPriceRaw}')");

            // Sanity check: subtotal = unit price * qty
            var subtotalRaw = await cart.GetFirstItemSubtotalRawAsync();
            var subtotal = CartPage.ParsePrice(subtotalRaw ?? string.Empty);
            Assert.That(subtotal, Is.EqualTo(unitPrice * qty), "Subtotal mismatch.");
        }
    }
}
