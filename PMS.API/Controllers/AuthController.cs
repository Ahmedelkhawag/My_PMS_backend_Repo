using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;

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
        [ProducesResponseType(typeof(ApiResponse<AuthModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterEmployee([FromForm] RegisterEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterEmployeeAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(new ApiResponse<string>(result.Message));
            return Ok(new ApiResponse<AuthModel>(result, result.Message));
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);

            if (!result.IsAuthenticated)
            {
                // حالة الفشل 👇
                return BadRequest(new ApiResponse<string>(result.Message));
            }

            // حالة النجاح 👇
            return Ok(new ApiResponse<AuthModel>(result, "Login successful"));
        }

        [HttpGet("roles")]
        [Authorize]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                // ممكن تعمل ميثود تستخرج الإيرورز من الـ ModelState وترجعها كـ List
                return BadRequest(new ApiResponse<string>("Invalid Data"));

            var result = await _authService.ChangePasswordAsync(model);

            if (!result.IsAuthenticated)
            {
                return BadRequest(new ApiResponse<string>(result.Message));
            }

            // هنا الـ Data بـ null لأننا مش محتاجين نرجع حاجة، بس الرسالة كفاية
            return Ok(new ApiResponse<string>(data: null, message: result.Message));
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _authService.GetCurrentUserProfileAsync();
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
        {
            var result = await _authService.UpdateCurrentUserProfileAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("users")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            var result = await _authService.GetAllUsersAsyncWithPagination(filter);
            return Ok(result);
        }

        [HttpGet("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _authService.GetUserByIdAsync(id);

            if (!result.Succeeded)
                return BadRequest(result); // أو NotFound لو حابب تفصل

            return Ok(result);
        }

        [HttpPost("users/{id}/reset-password")]
        [Authorize(Roles = "HotelManager,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ResetUserPassword(string id, [FromBody] AdminResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>("Invalid Data"));

            var result = await _authService.AdminForceResetPasswordAsync(id, model.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Message?.Contains("Access Denied") == true)
                    return StatusCode(403, result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("employees/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployee(string id, [FromForm] UpdateEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>("Invalid Data"));

            model.Id = id;
            var result = await _authService.UpdateEmployeeAsync(model);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _authService.DeleteUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("users/{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var result = await _authService.RestoreUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<StatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatuses()
        {
            var result = await _authService.GetStatusesAsync();
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(model.RefreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {

            var token = model.Token;

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Token is invalid or already revoked!");

            return Ok(new { message = "Token revoked successfully" });
        }
    }
}
