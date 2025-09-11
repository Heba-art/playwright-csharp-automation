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

    public async Task<bool> HasProductAsync(string productTitle)
    {
        var match = _page.Locator($".product-item .product-title a:has-text(\"{productTitle}\")").First;
        return await match.IsVisibleAsync();

    }
    }
}
