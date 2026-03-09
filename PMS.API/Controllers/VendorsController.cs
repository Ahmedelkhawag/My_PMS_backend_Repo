using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AP;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/vendors")]
    [ApiController]
    [Authorize(Roles = "Accountant,HotelManager,SuperAdmin")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        /// <summary>Gets a paged list of vendors with optional search.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VendorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 20,
            [FromQuery] string? search = null)
        {
            var result = await _vendorService.GetAllVendorsAsync(pageNumber, pageSize, search);
            return Ok(result);
        }

        /// <summary>Gets a single vendor by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vendorService.GetVendorByIdAsync(id);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>Creates a new vendor.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<VendorDto>("Invalid vendor payload."));

            var result = await _vendorService.CreateVendorAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Updates an existing vendor.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<VendorDto>("Invalid vendor payload."));

            var result = await _vendorService.UpdateVendorAsync(id, dto);

            if (!result.Succeeded)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>Soft-deletes a vendor (only if no linked invoices or payments).</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _vendorService.DeleteVendorAsync(id);

            if (!result.Succeeded)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
