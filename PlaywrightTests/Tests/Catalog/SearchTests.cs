using Microsoft.Playwright;
using PlaywrightTests.Pages;
using PlaywrightTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PlaywrightTests.Tests.Catalog
{
    [TestFixture]
    public class SearchTests : TestBase
    {
        [Test, Order(4)]
        public async Task SearchProduct_ExactMatch_ShouldShowProduct()
        {
        const string target = "Build your own computer";

        // Arrange – go to home
        var home = new HomePage(_page, _baseUrl);

        //Act – search for exact product name
        var results = await home.SearchAsync(target);

        //Assert – product is present in the results
        // 1) Soft/Playwright-style assertion on the titles list (more resilient)
        await Microsoft.Playwright.Assertions.Expect(results.ProductTitles).ToContainTextAsync(new [] { target });

        // 2) Hard/NUnit assertion using a helper
        Assert.That(await results.HasProductAsync(target),Is.True, $"Product '{target}' was not found in search results.");

        }
        [Test, Order(10)]
        public async Task SortProducts_PriceLowToHigh_ShouldDisplayAscendingPrices()
        {
            // Arrange - Open Notebooks category
            await _page.GotoAsync($"{_baseUrl}/notebooks");

            var productsPage = new SearchResultsPage(_page);

            // Act - Sort products by price: Low to High
            await productsPage.SortByPriceLowToHighAsync();
            //await _page.WaitForTimeoutAsync(5000);

            // Read prices as displayed on the website
            var actualPrices = await productsPage.GetProductPricesAsync();

            // Make sure we have enough products to test sorting
            Assert.That(
                actualPrices.Count,
                Is.GreaterThan(1),
                "Not enough products were found to verify sorting.");

            // Create the expected correctly sorted list
            var expectedPrices = actualPrices
                .OrderBy(price => price)
                .ToList();

            // Assert - Website prices should already be in ascending order
            Assert.That(
                actualPrices,
                Is.EqualTo(expectedPrices),
                $"Products are not sorted Low to High. Actual: {string.Join(", ", actualPrices)}");
        }
    }   
}
