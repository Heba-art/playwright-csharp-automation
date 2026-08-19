using Microsoft.Playwright;
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
            var home = new HomePage(_page,_baseUrl);

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
            await _page.Locator(".bar-notification.success").WaitForAsync(new() { State = WaitForSelectorState.Visible });

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
        [Test, Order(6)]
        public async Task UpdateQuantityInCart_ShouldRecalculateSubtotal()
        {
            const string product = "Build your own computer";
            const int newQuantity = 3;

            // Arrange - Add a product to the cart
            var home = new HomePage(_page, _baseUrl);

            var resultsPage = await home.SearchAsync(product);

            var productLink = resultsPage
                .ProductTitles
                .Filter(new() { HasText = product })
                .First;

            await productLink.ClickAsync();

            var productPage = new ProductPage(_page);

            await productPage.ApplyBaseConfigurationAsync();
            await productPage.WaitForPriceAsync(expected: 1250m, timeoutMs: 15000);

            await productPage.AddToCartAsync();

            await _page.Locator(".bar-notification.success")
                .WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Open cart
            await home.OpenCartAsync();

            var cart = new CartPage(_page);
            await cart.WaitForLoadedAsync();

            Assert.That(await cart.IsCartEmpty(), Is.False,
                "Cart should contain the added product.");

            // Get unit price before changing quantity
            var unitPriceRaw = await cart.GetFirstItemUnitPriceRawAsync();
            var unitPrice = CartPage.ParsePrice(unitPriceRaw);

            // Act - Change quantity from 1 to 3
            await cart.UpdateFirstItemQuantityAsync(newQuantity);

            // Assert - Quantity updated
            var actualQuantity = await cart.GetFirstItemQtyAsync();

            Assert.That(actualQuantity, Is.EqualTo(newQuantity),
                "Cart quantity was not updated correctly.");

            // Assert - Subtotal recalculated correctly
            var subtotalRaw = await cart.GetFirstItemSubtotalRawAsync();
            var subtotal = CartPage.ParsePrice(subtotalRaw);

            var expectedSubtotal = unitPrice * newQuantity;

            Assert.That(subtotal, Is.EqualTo(expectedSubtotal),
                $"Subtotal should be {unitPrice} × {newQuantity} = {expectedSubtotal}.");
        }
        [Test, Order(7)]
        public async Task RemoveItemFromCart_ShouldShowEmptyCart()
        {
            const string product = "Build your own computer";

            // Arrange - Add product to cart
            var home = new HomePage(_page, _baseUrl);

            var resultsPage = await home.SearchAsync(product);

            var productLink = resultsPage
                .ProductTitles
                .Filter(new() { HasText = product })
                .First;

            await productLink.ClickAsync();

            var productPage = new ProductPage(_page);

            await productPage.ApplyBaseConfigurationAsync();
            await productPage.WaitForPriceAsync(expected: 1250m, timeoutMs: 15000);

            await productPage.AddToCartAsync();

            await _page.Locator(".bar-notification.success")
                .WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Open cart
            await home.OpenCartAsync();

            var cart = new CartPage(_page);
            await cart.WaitForLoadedAsync();

            // Verify cart contains the product
            Assert.That(await cart.IsCartEmpty(), Is.False,
                "Cart should contain the added product.");

            // Act - Remove first item
            await cart.RemoveFirstItemAsync();
            await _page.WaitForTimeoutAsync(3000);

            // Assert - Cart should now be empty
            Assert.That(await cart.IsCartEmpty(), Is.True,
                "Cart should be empty after removing the product.");
        }
        [Test, Order(8)]
        public async Task WishlistToCart_ShouldMoveProductSuccessfully()
        {
            const string product = "Build your own computer";

            // Arrange - Search and open product
            var home = new HomePage(_page, _baseUrl);

            var resultsPage = await home.SearchAsync(product);

            var productLink = resultsPage
                .ProductTitles
                .Filter(new() { HasText = product })
                .First;

            await productLink.ClickAsync();

            var productPage = new ProductPage(_page);

            // Configure product before adding to Wishlist
            await productPage.ApplyBaseConfigurationAsync();

            // Add product to Wishlist
            await productPage.AddToWishlistAsync();

            // Open Wishlist
            await home.OpenWishlistAsync();

            var wishlist = new WishlistPage(_page);

            // Assert - Product exists in Wishlist
            var wishlistProductName = await wishlist.GetFirstItemNameAsync();

            Assert.That(
                wishlistProductName,
                Does.Contain(product),
                "Product was not found in Wishlist.");

            // Act - Tick checkbox and move product to Cart
            await wishlist.MoveFirstItemToCartAsync();

            // nopCommerce redirects automatically to the Shopping Cart

            var cart = new CartPage(_page);
            await cart.WaitForLoadedAsync();

            // Assert - Cart contains the moved product
            Assert.That(
                await cart.IsCartEmpty(),
                Is.False,
                "Cart should not be empty after moving product from Wishlist.");

            var cartProductName = await cart.GetFirstItemNameAsync();

            Assert.That(
                cartProductName,
                Does.Contain(product),
                "Wrong product was moved from Wishlist to Cart.");
        }
        [Test, Order(9)]
        public async Task CheckoutAsGuest_ShouldCompleteOrderSuccessfully()
        {
            const string product = "HTC smartphone";

            // Arrange - Search and open product
            var home = new HomePage(_page, _baseUrl);

            var resultsPage = await home.SearchAsync(product);

            var productLink = resultsPage
                .ProductTitles
                .Filter(new() { HasText = product })
                .First;

            await productLink.ClickAsync();

            var productPage = new ProductPage(_page);

            // Add product to Cart
            await productPage.AddToCartAsync();

            // Open Cart
            await home.OpenCartAsync();

            var cart = new CartPage(_page);
            await cart.WaitForLoadedAsync();

            Assert.That(await cart.IsCartEmpty(), Is.False,
                "Cart should contain the product before checkout.");

            // Accept terms and proceed to checkout
            await cart.ProceedToCheckoutAsync();

            var checkout = new CheckoutPage(_page);

            // Checkout as Guest
            await checkout.ChooseGuestCheckoutAsync();

            // Billing
            await checkout.FillBillingAddressAsync();

            // Shipping
            await checkout.ChooseShippingMethodAsync();

            // Payment method
            await checkout.ChoosePaymentMethodAsync();

            // Payment information
            await checkout.FillPaymentInformationAsync();

            // Confirm order
            await checkout.ConfirmOrderAsync();

            // Assert - Order completed successfully
            await checkout.WaitForOrderSuccessAsync();
        }
    }
}
