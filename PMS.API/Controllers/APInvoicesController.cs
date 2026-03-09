using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/ap/invoices")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class APInvoicesController : ControllerBase
    {
        private readonly IAPInvoiceService _invoiceService;

        public APInvoicesController(IAPInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>Gets a paged list of AP Invoices, optionally filtered by vendor.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<APInvoiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 20,
            [FromQuery] int? vendorId  = null)
        {
            var result = await _invoiceService.GetAllInvoicesAsync(pageNumber, pageSize, vendorId);
            return Ok(result);
        }

        /// <summary>Gets a single AP Invoice by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _invoiceService.GetInvoiceByIdAsync(id);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result);
        }

        /// <summary>Creates a new AP Invoice in Draft status.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAPInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<APInvoiceDto>("Invalid invoice payload."));

            var result = await _invoiceService.CreateInvoiceAsync(dto);
            if (!result.Succeeded) return BadRequest(result);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Approves a Draft invoice and posts it to the General Ledger.</summary>
        [HttpPost("{id:int}/approve")]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<APInvoiceDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _invoiceService.ApproveInvoiceAsync(id);
            if (!result.Succeeded)
            {
                if (result.Message?.Contains("not found") == true) return NotFound(result);
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>Voids an invoice. Reverses GL entry if the invoice was Approved.</summary>
        [HttpPost("{id:int}/void")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Void(int id)
        {
            var result = await _invoiceService.VoidInvoiceAsync(id);
            if (!result.Succeeded)
            {
                if (result.Message?.Contains("not found") == true) return NotFound(result);
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
