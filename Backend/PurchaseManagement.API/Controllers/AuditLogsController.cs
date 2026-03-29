using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.API.Services;

namespace PurchaseManagement.API.Controllers
{
    /// <summary>
    /// Controller for Audit Log retrieval.
    /// Implements Task 5.4 - Audit Trail (MANDATORY).
    /// Tracks Create and Update operations on Purchase Bills.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the most recent audit log entries across all entities.
        /// Supports optional pageSize parameter (default: 50).
        /// GET /api/auditlogs?pageSize=50
        /// </summary>
        

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageSize = 50)
        {
            _logger.LogInformation("Fetching audit logs (pageSize={PageSize})", pageSize);
            var logs = await _auditLogService.GetAuditLogsAsync(pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Returns audit logs filtered by entity name and optionally by entity record ID.
        /// Example: GET /api/auditlogs/PurchaseBill?entityId=5
        /// </summary>
        
        [HttpGet("{entity}")]
        public async Task<IActionResult> GetByEntity(string entity, [FromQuery] int? entityId = null)
        {
            _logger.LogInformation("Fetching audit logs for entity={Entity}, entityId={EntityId}", entity, entityId);
            var logs = await _auditLogService.GetAuditLogsByEntityAsync(entity, entityId);
            return Ok(logs);
        }
    }
}
