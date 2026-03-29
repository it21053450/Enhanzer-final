using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.API.Services;

namespace PurchaseManagement.API.Controllers
{
    /// <summary>
    /// Controller for Item master data.
    /// Implements Task 2: GET /api/items
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
        {
            _itemService = itemService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the full list of active items.
        /// Used to populate the Item autocomplete field in the Purchase Bill form.
        /// GET /api/items
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            _logger.LogInformation("Fetching all items");
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Returns items matching a search query string.
        /// Used for real-time autocomplete in the Purchase Bill form.
        /// GET /api/items/search?q={query}
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search query cannot be empty.");

            var items = await _itemService.SearchItemsAsync(q);
            return Ok(items);
        }
    }
}
