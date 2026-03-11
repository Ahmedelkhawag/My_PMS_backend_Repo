using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.BackOffice.AR;
using PMS.Application.DTOs.Common;
using PMS.Application.Exceptions;
using PMS.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.API.Controllers.BackOffice
{
    [ApiController]
    [Route("api/ar")]
    [Authorize]
    public class ARPaymentsController : ControllerBase
    {
        private readonly IARPaymentService _paymentService;
        private readonly ICreditService _creditService;

        public ARPaymentsController(IARPaymentService paymentService, ICreditService creditService)
        {
            _paymentService = paymentService;
            _creditService = creditService;
        }

        [HttpPost("payments/advance")]
        public async Task<ActionResult<ApiResponse<int>>> RecordAdvanceDeposit([FromBody] ProcessPaymentDto dto)
        {
            try
            {
                // Note: Advance deposits usually have a null InvoiceId.
                var result = await _paymentService.ProcessPaymentAsync(dto);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<int>(ex.Message));
            }
        }

        [HttpPost("allocations/apply")]
        public async Task<ActionResult<ApiResponse<bool>>> ApplyAllocation(int paymentId, [FromBody] List<AllocationRequest> requests)
        {
            try
            {
                var result = await _paymentService.AllocatePaymentAsync(paymentId, requests);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<bool>(ex.Message));
            }
        }

        [HttpGet("companies/{id}/credit-status")]
        public async Task<ActionResult<ApiResponse<CreditEligibilityResult>>> GetCreditStatus(int id, [FromQuery] decimal newInvoiceAmount = 0)
        {
            var result = await _creditService.CheckCreditEligibilityAsync(id, newInvoiceAmount);
            return Ok(new ApiResponse<CreditEligibilityResult>(result, "Credit eligibility retrieved successfully."));
        }
    }
}
