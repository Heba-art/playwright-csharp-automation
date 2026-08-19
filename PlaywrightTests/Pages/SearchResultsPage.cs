using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class SearchResultsPage
    {
    private readonly IPage _page;
    public SearchResultsPage(IPage page) => _page = page;
    // All product cards shown in search results
    public ILocator ProductCards => _page.Locator(".product-item");
    //Links/titles of the products
    public ILocator ProductTitles => _page.Locator(".product-item .product-title a");
    public ILocator SortDropdown => _page.Locator("#products-orderby");

     public ILocator ProductPrices => _page.Locator(".product-item .actual-price");

        public async Task<bool> HasProductAsync(string productTitle)
    {
        var match = _page.Locator($".product-item .product-title a:has-text(\"{productTitle}\")").First;
        return await match.IsVisibleAsync();

    }
        public async Task SortByPriceLowToHighAsync()
        {
            // Save the current prices before sorting
            var oldPrices = (await ProductPrices.AllInnerTextsAsync())
                .Select(price => price.Trim())
                .ToArray();

            // Select Price: Low to High
            await SortDropdown.SelectOptionAsync(
                new SelectOptionValue { Label = "Price: Low to High" });

            // Wait until the URL confirms the selected sorting
            await _page.WaitForURLAsync(
                url => url.Contains("orderby=10"),
                new() { Timeout = 10000 });

            // Wait until the displayed product prices actually refresh
            await _page.WaitForFunctionAsync(
                @"oldPrices => {
            const currentPrices = Array.from(
                document.querySelectorAll('.product-item .actual-price')
            ).map(x => x.textContent.trim());

            return currentPrices.length > 0 &&
                   JSON.stringify(currentPrices) !== JSON.stringify(oldPrices);
        }",
                oldPrices,
                new() { Timeout = 10000 });
        }
        public async Task<List<decimal>> GetProductPricesAsync()
        {
            var prices = new List<decimal>();

            var count = await ProductPrices.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var rawPrice = (await ProductPrices.Nth(i).InnerTextAsync()).Trim();

                var price = CartPage.ParsePrice(rawPrice);

                prices.Add(price);
            }

            return prices;
        }
    }
}
