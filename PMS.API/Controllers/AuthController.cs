using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;

namespace PMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-employee")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AuthModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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
        [Authorize] // أي حد مسجل دخول يقدر يشوف الرولات (ممكن تحددها لـ SuperAdmin,Manager بس)
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
        {
            var result = await _authService.UpdateCurrentUserProfileAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("users")] // GET: /api/auth/users?PageNumber=1&PageSize=10&Search=Ahmed
        [Authorize] // لازم يكون مسجل دخول طبعاً
        [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            var result = await _authService.GetAllUsersAsyncWithPagination(filter);
            return Ok(result);
        }

        [HttpGet("user/{id}")] // الرابط: api/v1/auth/user/{id}
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _authService.GetUserByIdAsync(id);

            if (!result.Succeeded)
                return BadRequest(result); // أو NotFound لو حابب تفصل

            return Ok(result);
        }

        [HttpPost("user/{id}/reset-password")]
        [Authorize(Roles = "HotelManager,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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

        [HttpPut("update-employee")]
        [Authorize]
        // استخدمنا FromForm عشان متوقعين ملفات
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmployee([FromForm] UpdateEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>("Invalid Data"));

            var result = await _authService.UpdateEmployeeAsync(model);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("user/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _authService.DeleteUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("user/{id}/restore")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var result = await _authService.RestoreUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("statuses")] // API: GET /api/auth/statuses
        [ProducesResponseType(typeof(List<StatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatuses()
        {
            var result = await _authService.GetStatusesAsync();
            return Ok(result);
        }
    }
}
