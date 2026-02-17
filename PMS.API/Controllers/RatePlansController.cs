using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;
using PMS.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [Route("api/rate-plans")]
    [ApiController]
    [Authorize]
    public class RatePlansController : ControllerBase
    {
        private readonly IRatePlanService _ratePlanService;

        public RatePlansController(IRatePlanService ratePlanService)
        {
            _ratePlanService = ratePlanService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseObjectDto<List<RatePlanDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] bool? isPublicOnly)
        {
            var result = await _ratePlanService.GetAllAsync(isPublicOnly);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ResponseObjectDto<RatePlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _ratePlanService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseObjectDto<RatePlanDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRatePlanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>("Validation failed"));
            }

            var result = await _ratePlanService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ResponseObjectDto<RatePlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRatePlanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>("Validation failed"));
            }

            var result = await _ratePlanService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ratePlanService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/restore")]
        [ProducesResponseType(typeof(ResponseObjectDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _ratePlanService.RestoreAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

