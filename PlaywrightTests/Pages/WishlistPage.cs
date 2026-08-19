using Microsoft.Playwright;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages
{
    public class WishlistPage
    {
        private readonly IPage _page;

        public WishlistPage(IPage page) => _page = page;

        public ILocator WishlistTable => _page.Locator("table.cart");

        public ILocator FirstRowProductName => WishlistTable.Locator("td.product a.product-name").First;

        public ILocator FirstRowAddToCartCheckbox => WishlistTable.Locator("input[type='checkbox']").First;

        public ILocator AddToCartButton => _page.GetByRole(AriaRole.Button,
                new() { Name = "Add to cart", Exact = true });

        public async Task<string> GetFirstItemNameAsync()
        {
            return (await FirstRowProductName.InnerTextAsync()).Trim();
        }

        public async Task MoveFirstItemToCartAsync()
        {
            await FirstRowAddToCartCheckbox.CheckAsync();
            await AddToCartButton.ClickAsync();
        }
    }
}