using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Companies;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _companyService.CreateCompanyAsync(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);

            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<CompanyProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _companyService.GetAllCompaniesAsync(search, pageNumber, pageSize);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<CompanyProfileDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyService.GetCompanyByIdAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyProfileDto dto)
        {
            if (dto != null && dto.Id.HasValue && dto.Id.Value != id)
                return BadRequest("Route id does not match request body id.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _companyService.UpdateCompanyAsync(id, dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _companyService.DeleteCompanyAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }
    }
}
