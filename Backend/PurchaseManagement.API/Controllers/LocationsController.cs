using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.API.Services;

namespace PurchaseManagement.API.Controllers
{
    /// <summary>
    /// Controller for Location master data.
    /// Implements Task 2: GET /api/locations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Returns list of all active locations (warehouses/stores).
        /// Used to populate the Batch dropdown in the Purchase Bill form.
        /// GET /api/locations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            _logger.LogInformation("Fetching all locations");
            var locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }
    }
}
