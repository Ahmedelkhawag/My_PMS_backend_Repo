using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/ap-payments")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class APPaymentsController : ControllerBase
    {
        private readonly IAPPaymentService _paymentService;

        public APPaymentsController(IAPPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>Gets a paged list of AP Payments, optionally filtered by vendor.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<APPaymentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? vendorId = null)
        {
            var result = await _paymentService.GetAllPaymentsAsync(pageNumber, pageSize, vendorId);
            return Ok(result);
        }

        /// <summary>Gets a single AP Payment by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<APPaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<APPaymentDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result);
        }

        /// <summary>Creates a new AP Payment with allocations.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<APPaymentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<APPaymentDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAPPaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<APPaymentDto>("Invalid payment payload."));

            var result = await _paymentService.CreatePaymentAsync(dto);
            if (!result.Succeeded) return BadRequest(result);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Voids an AP Payment, reverses GL and updates linked invoices.</summary>
        [HttpPost("{id:int}/void")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Void(int id)
        {
            var result = await _paymentService.VoidPaymentAsync(id);
            if (!result.Succeeded)
            {
                if (result.Message?.Contains("not found") == true) return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

