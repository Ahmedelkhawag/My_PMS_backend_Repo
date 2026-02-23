using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using System.Security.Claims;

namespace PMS.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("employees")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<AuthModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterEmployee([FromForm] RegisterEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات غير صالحة", 400));

            var result = await _authService.RegisterEmployeeAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));

            return Ok(ResponseObjectDto<AuthModel>.Success(result, result.Message));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ResponseObjectDto<AuthModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("بيانات الدخول غير صحيحة", 400));

            var result = await _authService.LoginAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));

            return Ok(ResponseObjectDto<AuthModel>.Success(result, "تم تسجيل الدخول بنجاح"));
        }

        [HttpGet("roles")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<List<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(ResponseObjectDto<List<string>>.Success(roles, "تم جلب الصلاحيات بنجاح"));
        }

        [HttpPost("password")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("البيانات المبعوتة غلط", 400));

            var result = await _authService.ChangePasswordAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));

            return Ok(ResponseObjectDto<string>.Success(null, result.Message));
        }

        [HttpPost("verify-password")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("كلمة المرور مطلوبة", 400));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ResponseObjectDto<string>.Failure("يجب تسجيل الدخول أولاً", 401));

            var result = await _authService.VerifyCurrentPasswordAsync(userId, model.Password);

            if (!result.Succeeded)
                return BadRequest(ResponseObjectDto<string>.Failure("كلمة المرور غير صحيحة", 400));

            return Ok(ResponseObjectDto<object>.Success(result, "تم التحقق من كلمة المرور"));
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<UserDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _authService.GetCurrentUserProfileAsync();
            if (!result.Succeeded)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));

            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
        {
            var result = await _authService.UpdateCurrentUserProfileAsync(model);
            if (!result.Succeeded)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));

            return Ok(result);
        }

        [HttpGet("users")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<PagedResult<UserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _authService.GetAllUsersAsync(search, pageNumber, pageSize);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 400, result);
            return Ok(result);
        }

        [HttpGet("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<UserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _authService.GetUserByIdAsync(id);
            if (!result.Succeeded)
                return NotFound(ResponseObjectDto<string>.Failure("المستخدم غير موجود", 404));

            return Ok(result);
        }

        [HttpPost("users/{id}/reset-password")]
        [Authorize(Roles = "HotelManager,SuperAdmin")]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetUserPassword(string id, [FromBody] AdminResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("البيانات غير صالحة", 400));

            var result = await _authService.AdminForceResetPasswordAsync(id, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("employees/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateEmployee(string id, [FromForm] UpdateEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseObjectDto<string>.Failure("البيانات غير صالحة", 400));

            var result = await _authService.UpdateEmployeeAsync(id, model);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _authService.DeleteUserAsync(id);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("users/{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var result = await _authService.RestoreUserAsync(id);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("statuses")]
        [ProducesResponseType(typeof(ResponseObjectDto<List<StatusDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatuses()
        {
            var result = await _authService.GetStatusesAsync();
            return Ok(ResponseObjectDto<List<StatusDto>>.Success(result, "تم جلب الحالات بنجاح"));
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ResponseObjectDto<AuthModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto model)
        {
            var result = await _authService.RefreshTokenAsync(model.RefreshToken);
            if (!result.IsAuthenticated)
                return BadRequest(ResponseObjectDto<string>.Failure(result.Message, 400));
            return Ok(ResponseObjectDto<AuthModel>.Success(result, "تم تحديث التوكن"));
        }

        [HttpPost("revoke-token")]
        [ProducesResponseType(typeof(ResponseObjectDto<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {
            if (string.IsNullOrEmpty(model.Token))
                return BadRequest(ResponseObjectDto<string>.Failure("التوكن مطلوب", 400));

            var result = await _authService.RevokeTokenAsync(model.Token);
            if (!result)
                return BadRequest(ResponseObjectDto<string>.Failure("التوكن غير صالح أو منتهي", 400));

            return Ok(ResponseObjectDto<string>.Success(null, "تم إلغاء التوكن بنجاح"));
        }
    }
}
