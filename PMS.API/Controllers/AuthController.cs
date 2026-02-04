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
        [Authorize] // لازم يكون مسجل دخول عشان نعرف هو تبع انهي فندق
                    // [Authorize(Roles = "SuperAdmin,Manager")] // ممكن نحدد رولات معينة لو حابب قدام
        public async Task<IActionResult> RegisterEmployee([FromForm] RegisterEmployeeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterEmployeeAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(new ApiResponse<string>(result.Message));
            return Ok(result);
        }


        [HttpPost("login")]
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
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("change-password")]
        [Authorize]
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

        [HttpGet("users")] // الرابط: api/v1/auth/users
        [Authorize] // لازم يكون مسجل دخول طبعاً
        public async Task<IActionResult> GetAllUsers()
        {
            // ممكن هنا تضيف Role Check لو عايز تمنع موظفين معينين يشوفوا القائمة
            // مثلاً: [Authorize(Roles = "Manager,Admin")]

            var users = await _authService.GetAllUsersAsync();
            var response = new ApiResponse<List<UserResponseDto>>(users, "Users retrieved successfully");
            // بنرجعهم في شكل JSON نظيف
            return Ok(response);
        }

        [HttpGet("user/{id}")] // الرابط: api/v1/auth/user/{id}
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _authService.GetUserByIdAsync(id);

            if (!result.Succeeded)
                return BadRequest(result); // أو NotFound لو حابب تفصل

            return Ok(result);
        }

        [HttpPut("update-employee")]
        [Authorize]
        // استخدمنا FromForm عشان متوقعين ملفات
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
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _authService.DeleteUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("user/{id}/restore")]
        [Authorize]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var result = await _authService.RestoreUserAsync(id);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
