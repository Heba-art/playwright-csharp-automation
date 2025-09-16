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
        await home.GoToAsync(_baseUrl);

        //Act – search for exact product name
        var results = await home.SearchAsync(target);

        //Assert – product is present in the results
        // 1) Soft/Playwright-style assertion on the titles list (more resilient)
        await Microsoft.Playwright.Assertions.Expect(results.ProductTitles).ToContainTextAsync(new [] { target });

        // 2) Hard/NUnit assertion using a helper
        Assert.That(await results.HasProductAsync(target),Is.True, $"Product '{target}' was not found in search results.");

        }
    }   
}
