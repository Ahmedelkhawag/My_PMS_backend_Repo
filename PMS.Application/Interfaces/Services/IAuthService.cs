using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterEmployeeAsync(RegisterEmployeeDto model);
        Task<AuthModel> LoginAsync(LoginDto model);
        Task<List<string>> GetRolesAsync();

        Task<AuthModel> ChangePasswordAsync(ChangePasswordDto model);

        Task<List<UserResponseDto>> GetAllUsersAsync();

        Task<ApiResponse<UserDetailDto>> GetUserByIdAsync(string userId);

        Task<ApiResponse<UserDetailDto>> GetCurrentUserProfileAsync();

        Task<ApiResponse<string>> UpdateCurrentUserProfileAsync(UpdateProfileDto model);

        Task<ApiResponse<string>> UpdateEmployeeAsync(UpdateEmployeeDto model);

        Task<ApiResponse<string>> DeleteUserAsync(string userId);

        Task<ApiResponse<string>> RestoreUserAsync(string userId);

        Task<ApiResponse<string>> AdminForceResetPasswordAsync(string targetUserId, string newPassword);

        Task<PagedResult<UserResponseDto>> GetAllUsersAsyncWithPagination(UserFilterDto filter);

        Task<List<StatusDto>> GetStatusesAsync();
    }
}
