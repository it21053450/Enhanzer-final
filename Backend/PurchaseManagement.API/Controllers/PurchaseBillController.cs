using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.API.DTOs;
using PurchaseManagement.API.Services;

namespace PurchaseManagement.API.Controllers
{
    /// <summary>
    /// Controller for Purchase Bill operations.
    /// Implements:
    ///   Task 4: POST /api/purchase-bill  (Create)
    ///   Task 5.2: PUT /api/purchase-bill/{id} (Edit/Update)
    ///   Supporting: GET /api/purchase-bill (List)
    ///               GET /api/purchase-bill/{id} (Single)
    /// </summary>
    [ApiController]
    [Route("api/purchase-bill")]
    public class PurchaseBillController : ControllerBase
    {
        private readonly IPurchaseBillService _billService;
        private readonly ILogger<PurchaseBillController> _logger;

        public PurchaseBillController(IPurchaseBillService billService, ILogger<PurchaseBillController> logger)
        {
            _billService = billService;
            _logger = logger;
        }

        /// <summary>
        /// Returns a summary list of all purchase bills.
        /// GET /api/purchase-bill
        /// </summary>
        /// 
    
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all purchase bills");
            var bills = await _billService.GetAllBillsAsync();
            return Ok(bills);
        }

        /// <summary>
        /// Returns the full detail of a specific purchase bill including all line items.
        /// Used for loading a bill in Edit mode (Task 5.2) and PDF generation.
        /// GET /api/purchase-bill/{id}
        /// </summary>
        

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching purchase bill with ID {Id}", id);
            var bill = await _billService.GetBillByIdAsync(id);
            if (bill == null)
                return NotFound(new { message = $"Purchase bill with ID {id} not found." });

            return Ok(bill);
        }

        /// <summary>
        /// Creates a new Purchase Bill with header and line items.
        /// Automatically generates a bill number, calculates totals, and writes an audit log.
        /// POST /api/purchase-bill
        /// </summary>
        
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseBillDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating new purchase bill");
            var result = await _billService.CreateBillAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Updates an existing Purchase Bill (Task 5.2 - Edit Purchase Bill).
        /// Replaces all line items, recalculates totals, and writes an audit log.
        /// PUT /api/purchase-bill/{id}
        /// </summary>
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseBillDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Updating purchase bill with ID {Id}", id);
            var result = await _billService.UpdateBillAsync(id, dto);

            if (!result.Success)
                return result.Message.Contains("not found")
                    ? NotFound(result)
                    : BadRequest(result);

            return Ok(result);
        }
    }
}
