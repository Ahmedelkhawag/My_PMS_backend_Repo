using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.DTOs.Common;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Application.Settings;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using PMS.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<JWT> _jwt;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(
         UserManager<AppUser> userManager,
         RoleManager<IdentityRole> roleManager,
         IOptions<JWT> jwt,
         ApplicationDbContext context,
         IUnitOfWork unitOfWork,
         IHttpContextAccessor httpContextAccessor,
         IWebHostEnvironment webHostEnvironment,
         IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuthModel> RegisterEmployeeAsync(RegisterEmployeeDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "Username is already taken!" };

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return new AuthModel { Message = "Invalid Role selected." };
            }

            if (model.Role == "SuperAdmin")
            {
                return new AuthModel { Message = "Cannot assign SuperAdmin role to an employee." };
            }

            string? profileImgPath = null;
            if (model.ProfileImage != null)
            {
                profileImgPath = await SaveFileAsync(model.ProfileImage, "profile-images");
            }

            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                WorkNumber = model.WorkNumber,
                NationalId = model.NationalId,
                Nationality = model.Nationality,
                Gender = Enum.TryParse<PMS.Domain.Enums.Gender>(model.Gender, true, out var parsedGender) ? parsedGender : null,
                DateOfBirth = model.BirthdayDate,
                ProfileImagePath = profileImgPath,
                IsActive = model.IsActive,
                ChangePasswordApprove = model.ChangePasswordApprove
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description},";
                return new AuthModel { Message = errors };
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new AuthModel { Message = "Failed to assign role to user." };
            }

            if (model.EmployeeDocs != null && model.EmployeeDocs.Count > 0)
            {
                foreach (var file in model.EmployeeDocs)
                {
                    var docPath = await SaveFileAsync(file, "employee-docs");

                    var newDoc = new EmployeeDocument
                    {
                        FileName = file.FileName,
                        FileType = Path.GetExtension(file.FileName),
                        FilePath = docPath,
                        AppUserId = user.Id
                    };

                    await _unitOfWork.EmployeeDocuments.AddAsync(newDoc);
                }

                await _unitOfWork.CompleteAsync();
            }

            return new AuthModel
            {
                IsAuthenticated = true,
                Message = "Employee registered successfully",
                Email = user.Email,
                Username = user.UserName,
                Roles = new List<string> { model.Role }
            };
        }

        public async Task<AuthModel> LoginAsync(LoginDto model)
        {
            var user = await _userManager.Users
           .Include(u => u.Status)
           .SingleOrDefaultAsync(u => u.UserName == model.UserName);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Invalid Username or Password!" };

            if (!user.IsActive)
                return new AuthModel { Message = "User is Disabled!" };

            var token = await CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            refreshToken.AppUserId = user.Id;
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            return new AuthModel
            {
                IsAuthenticated = true,
                Token = token,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn,
                Email = user.Email,
                Username = user.UserName,
                ExpiresOn = DateTime.Now.AddMinutes(_jwt.Value.DurationInMinutes),
                Roles = (List<string>)await _userManager.GetRolesAsync(user),
                Message = "Login Successful",
                ChangePasswordApprove = user.ChangePasswordApprove,
                HotelId = user.HotelId
            };
        }

        public async Task<AuthModel> ChangePasswordAsync(ChangePasswordDto model)
        {

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new AuthModel { Message = "User not found or not logged in." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel { Message = "User not found." };

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description}, ";

                return new AuthModel { Message = errors };
            }

            user.ChangePasswordApprove = false;

            await _userManager.UpdateAsync(user);

            return new AuthModel
            {
                IsAuthenticated = true,
                Message = "Password changed successfully.",
                Username = user.UserName,
                Email = user.Email,
                Roles = (List<string>)await _userManager.GetRolesAsync(user)
            };
        }

        public async Task<ApiResponse<bool>> VerifyCurrentPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponse<bool>("User not found.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return new ApiResponse<bool>("Incorrect password.");

            return new ApiResponse<bool>(true, "Password verified successfully.");
        }

        public async Task<ResponseObjectDto<PagedResult<UserResponseDto>>> GetAllUsersAsync(string? search, int pageNumber, int pageSize)
        {
            var response = new ResponseObjectDto<PagedResult<UserResponseDto>>();

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(searchLower))
                );
            }

            var totalCount = await query.CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var skip = (pageNumber - 1) * pageSize;
            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var responseList = new List<UserResponseDto>();

            foreach (var user in users)
            {

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = _mapper.Map<UserResponseDto>(user);
                userDto.Role = roles.FirstOrDefault() ?? "Employee";
                responseList.Add(userDto);
            }

            var pagedResult = new PagedResult<UserResponseDto>(responseList, totalCount, pageNumber, pageSize);

            response.IsSuccess = true;
            response.StatusCode = 200;
            response.Message = "Users retrieved successfully";
            response.Data = pagedResult;

            return response;
        }

        public async Task<ApiResponse<UserDetailDto>> GetUserByIdAsync(string userId)
        {

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var targetUser = await _userManager.Users
                .Include(u => u.Status)
                .Include(u => u.EmployeeDocs)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (targetUser == null)
                return new ApiResponse<UserDetailDto>("User not found.");

            var roles = await _userManager.GetRolesAsync(targetUser);

            var userDetail = _mapper.Map<UserDetailDto>(targetUser);
            userDetail.Role = roles.FirstOrDefault() ?? "Employee";
            userDetail.DocumentPaths = targetUser.EmployeeDocs?.Select(d => d.FilePath).ToList() ?? new List<string>();

            return new ApiResponse<UserDetailDto>(userDetail, "User details retrieved successfully");
        }

        public async Task<ApiResponse<UserDetailDto>> GetCurrentUserProfileAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new ApiResponse<UserDetailDto>("User not found or not logged in.");

            var user = await _userManager.Users
                .Include(u => u.Status)
                .Include(u => u.EmployeeDocs)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new ApiResponse<UserDetailDto>("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var userDetail = _mapper.Map<UserDetailDto>(user);
            userDetail.Role = roles.FirstOrDefault() ?? "Employee";
            userDetail.DocumentPaths = user.EmployeeDocs?.Select(d => d.FilePath).ToList() ?? new List<string>();

            return new ApiResponse<UserDetailDto>(userDetail, "Profile retrieved successfully");
        }

        public async Task<ApiResponse<string>> UpdateCurrentUserProfileAsync(UpdateProfileDto model)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new ApiResponse<string>("User not found or not logged in.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponse<string>("User not found.");

            if (!string.IsNullOrEmpty(model.FullName))
                user.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.PhoneNumber))
                user.PhoneNumber = model.PhoneNumber;

            if (model.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(user.ProfileImagePath))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImagePath.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
                user.ProfileImagePath = await SaveFileAsync(model.ProfileImage, "profile-images");
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new ApiResponse<string>(updateResult.Errors.Select(e => e.Description).ToList(), "Failed to update profile.");

            return new ApiResponse<string>(data: null, "Profile updated successfully");
        }

        public async Task<ApiResponse<string>> AdminForceResetPasswordAsync(string targetUserId, string newPassword)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new ApiResponse<string>("User not found or not logged in.");

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (targetUser == null)
                return new ApiResponse<string>("User not found.");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = currentUserRoles.Contains(Roles.SuperAdmin);
            bool isHotelManager = currentUserRoles.Contains(Roles.HotelManager);

            if (isHotelManager && !isSuperAdmin)
            {
                if (currentUser.HotelId == null || targetUser.HotelId != currentUser.HotelId)
                    return new ApiResponse<string>("Access Denied: You can only reset passwords for users in your hotel.");
            }

            var removeResult = await _userManager.RemovePasswordAsync(targetUser);
            if (!removeResult.Succeeded)
                return new ApiResponse<string>(removeResult.Errors.Select(e => e.Description).ToList(), "Failed to remove existing password.");

            var addResult = await _userManager.AddPasswordAsync(targetUser, newPassword);
            if (!addResult.Succeeded)
                return new ApiResponse<string>(addResult.Errors.Select(e => e.Description).ToList(), "Password does not meet requirements.");

            targetUser.ChangePasswordApprove = true;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
                return new ApiResponse<string>(updateResult.Errors.Select(e => e.Description).ToList(), "Failed to update user.");

            return new ApiResponse<string>(data: null, "Password reset successfully. User must change password on next login.");
        }

        public async Task<ApiResponse<string>> UpdateEmployeeAsync(string id, UpdateEmployeeDto model)
        {

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var userToUpdate = await _userManager.FindByIdAsync(id);

            if (userToUpdate == null)
                return new ApiResponse<string>("User not found.");

            if (!string.IsNullOrEmpty(model.FullName)) userToUpdate.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.PhoneNumber)) userToUpdate.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrEmpty(model.NationalId)) userToUpdate.NationalId = model.NationalId;
            if (!string.IsNullOrEmpty(model.WorkNumber)) userToUpdate.WorkNumber = model.WorkNumber;
            if (!string.IsNullOrEmpty(model.Nationality)) userToUpdate.Nationality = model.Nationality;
            if (model.DateOfBirth.HasValue) userToUpdate.DateOfBirth = model.DateOfBirth.Value;
            if (model.IsActive.HasValue) userToUpdate.IsActive = model.IsActive.Value;

            if (model.ProfileImage != null)
            {

                if (!string.IsNullOrEmpty(userToUpdate.ProfileImagePath))
                {

                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, userToUpdate.ProfileImagePath.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                userToUpdate.ProfileImagePath = await SaveFileAsync(model.ProfileImage, "profile-images");
            }

            var updateResult = await _userManager.UpdateAsync(userToUpdate);
            if (!updateResult.Succeeded)
                return new ApiResponse<string>(updateResult.Errors.Select(e => e.Description).ToList(), "Failed to update user.");

            if (!string.IsNullOrEmpty(model.Role))
            {

                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);

                    if (!currentRoles.Contains(model.Role))
                    {

                        await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);

                        await _userManager.AddToRoleAsync(userToUpdate, model.Role);
                    }
                }
                else
                {
                    return new ApiResponse<string>("Invalid Role selected, other data updated successfully.");
                }
            }

            if (model.EmployeeDocs != null && model.EmployeeDocs.Count > 0)
            {
                foreach (var file in model.EmployeeDocs)
                {

                    var docPath = await SaveFileAsync(file, "employee-docs");

                    var newDoc = new EmployeeDocument
                    {
                        FileName = file.FileName,
                        FileType = Path.GetExtension(file.FileName),
                        FilePath = docPath,

                        AppUserId = userToUpdate.Id
                    };

                    await _unitOfWork.EmployeeDocuments.AddAsync(newDoc);
                }

                await _unitOfWork.CompleteAsync();
            }

            return new ApiResponse<string>(data: null, "User updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteUserAsync(string userId)
        {

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var userToDelete = await _userManager.FindByIdAsync(userId);

            if (userToDelete == null)
                return new ApiResponse<string>("User not found.");

            userToDelete.IsDeleted = true;
            userToDelete.DeletedAt = DateTime.UtcNow;
            userToDelete.DeletedBy = currentUserId; 

            var result = await _userManager.UpdateAsync(userToDelete);

            if (!result.Succeeded)
                return new ApiResponse<string>("Failed to delete user.");

            return new ApiResponse<string>(data: null, "User deleted successfully");
        }

        public async Task<ApiResponse<string>> RestoreUserAsync(string userId)
        {

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var userToRestore = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToRestore == null)
                return new ApiResponse<string>("User not found or not deleted.");

            if (!userToRestore.IsDeleted)
                return new ApiResponse<string>("User is not deleted.");

            userToRestore.IsDeleted = false;
            userToRestore.DeletedAt = null;
            userToRestore.DeletedBy = null;

            var result = await _userManager.UpdateAsync(userToRestore);

            if (!result.Succeeded)
                return new ApiResponse<string>("Failed to restore user.");

            return new ApiResponse<string>(data: null, "User restored successfully");
        }

        public async Task<PagedResult<UserResponseDto>> GetAllUsersAsyncWithPagination(UserFilterDto filter)
        {

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    u.PhoneNumber.Contains(s) ||
                    u.UserName.ToLower().Contains(s)
                );
            }

            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                {
                    query = query.Where(u => u.IsActive);
                }
                else
                {
                    query = query.Where(u => !u.IsActive);
                }
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {

                var roleId = await _context.Roles
                    .Where(r => r.Name == filter.Role)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (roleId != null)
                {

                    var userIdsInRole = _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId);

                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
                else
                {

                    query = query.Where(u => false);
                }
            }

            var totalCount = await query.CountAsync();

            var pagedUsers = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responseList = new List<UserResponseDto>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                responseList.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? "Employee",
                    HotelId = user.HotelId,
                    CreatedBy = user.CreatedBy,
                    CreatedAt = user.CreatedAt,
                    UpdatedBy = user.LastModifiedBy,
                    UpdatedAt = user.LastModifiedAt
                });
            }

            return new PagedResult<UserResponseDto>(responseList, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<List<StatusDto>> GetStatusesAsync()
        {
            var statuses = await _context.Statuses
                .Select(s => new StatusDto
                {
                    Id = s.StatusID,
                    Name = s.Name
                })
                .ToListAsync();

            return statuses;
        }

        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var storedRefreshToken = await _context.RefreshTokens
         .Include(r => r.AppUser)
         .SingleOrDefaultAsync(t => t.Token == token);

            if (storedRefreshToken == null)
            {
                authModel.Message = "Invalid Token";
                return authModel;
            }

            if (!storedRefreshToken.IsActive)
            {
                authModel.Message = "Inactive Token";
                return authModel;
            }

            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.AppUserId = storedRefreshToken.AppUserId;

            var user = storedRefreshToken.AppUser;
            var newJwtToken = await CreateJwtToken(user);

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            _unitOfWork.RefreshTokens.Update(storedRefreshToken);
            await _unitOfWork.CompleteAsync();

            return new AuthModel
            {
                IsAuthenticated = true,
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn,
                Email = user.Email,
                Username = user.UserName,
                Roles = (List<string>)await _userManager.GetRolesAsync(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
         .Include(r => r.AppUser)
         .SingleOrDefaultAsync(t => t.Token == token);

            if (refreshToken == null)
                return false;

            if (!refreshToken.IsActive)
                return true;

            refreshToken.RevokedOn = DateTime.UtcNow;

            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private async Task<string> CreateJwtToken(AppUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("HotelId", user.HotelId?.ToString() ?? ""),
                new Claim("change_password_approve", user.ChangePasswordApprove.ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Value.Key));

            var token = new JwtSecurityToken(
                issuer: _jwt.Value.Issuer,
                audience: _jwt.Value.Audience,
                expires: DateTime.Now.AddMinutes(_jwt.Value.DurationInMinutes),
                claims: userClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<string>> GetRolesAsync()
        {

            var roles = await _roleManager.Roles

                .Where(r => r.Name != "SuperAdmin")
                .Select(r => r.Name)
                .ToListAsync();

            return roles;
        }
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {

            string webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddHours(_jwt.Value.RefreshTokenValidityInHours),
                CreatedOn = DateTime.UtcNow
            };
        }

    }
}

