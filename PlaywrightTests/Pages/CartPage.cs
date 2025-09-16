using Microsoft.Playwright;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class CartPage
    {
        private readonly IPage _page;

        public CartPage(IPage page) => _page = page;

        // Locators for cart elements
        public ILocator CartTable => _page.Locator("table.cart");
        public ILocator EmptyCartMessage => _page.Locator("div.order-summary-content", new() { HasText = "Your shopping cart is empty!" });

        // Helpers to fetch columns for the first cart row
        public ILocator FirstRowProductName => CartTable.Locator("td.product-name").First;
        public ILocator FirstRowUnitPrice => CartTable.Locator("td.unit-price").First;
        public ILocator FirstRowQtyInput => CartTable.Locator("input.qty-input").First;
        public ILocator FirstRowSubtotal => CartTable.Locator("td.subtotal").First;

        public async Task<string> GetFirstItemNameAsync()
        {
            var row = _page.Locator("table.cart tbody tr").First;
            var name = row.Locator("td.product a.product-name, td.product a").First;
            return (await name.InnerTextAsync()).Trim();
        }

        public async Task<string> GetFirstItemUnitPriceRawAsync()
        {
            var row = _page.Locator("table.cart tbody tr").First;
            return (await row.Locator("td.unit-price").InnerTextAsync()).Trim();
        }
        public async Task<string> GetFirstItemSubtotalRawAsync()
        {
            var row = _page.Locator("table.cart tbody tr").First;
            return (await row.Locator("td.subtotal").InnerTextAsync()).Trim();
        }

        public async Task<int> GetFirstItemQtyAsync()
        {
            var row = _page.Locator("table.cart tbody tr").First;
            var val = await row.Locator("input.qty-input").InputValueAsync();
            return int.Parse(val);
        }

        // Parse price from string (e.g., "$1,250.00" to 1250.00)
        public static decimal ParsePrice(string price)
        {
            var cleaned = System.Text.RegularExpressions.Regex.Replace(price ?? "", @"[^\d\.\-]", "");
            return decimal.Parse(cleaned, System.Globalization.CultureInfo.InvariantCulture);
        }

        public async Task WaitForLoadedAsync(int timeoutMs = 15000)
        {
            var tableTask = CartTable.First.WaitForAsync(new() { Timeout = timeoutMs });
            var emptyTask = _page.GetByText("Your Shopping Cart is empty", new() { Exact = false })
                                 .WaitForAsync(new() { Timeout = timeoutMs });
            await Task.WhenAny(tableTask, emptyTask);
        }

        // IsCartEmpty method
        public async Task<bool> IsCartEmpty()
         => await _page.GetByText("Your Shopping Cart is empty", new() { Exact = false }).CountAsync() > 0;
    }
}