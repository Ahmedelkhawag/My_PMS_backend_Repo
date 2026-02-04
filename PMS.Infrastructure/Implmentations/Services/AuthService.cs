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
using PMS.Domain.Entities;
using PMS.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<JWT> _jwt;
        private readonly ApplicationDbContext _context; // محتاجينه عشان نحفظ المستندات
        private readonly IHttpContextAccessor _httpContextAccessor; // جديد
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;

        // بنحقن الحاجات اللي محتاجينها
        public AuthService(
         UserManager<AppUser> userManager,
         RoleManager<IdentityRole> roleManager,
         IOptions<JWT> jwt,
         ApplicationDbContext context,
         IUnitOfWork unitOfWork,
         IHttpContextAccessor httpContextAccessor,
         IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthModel> RegisterEmployeeAsync(RegisterEmployeeDto model)
        {
            // 1. التأكد إن مفيش حد بنفس البيانات دي
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "Username is already taken!" };

            // 2. استخراج HotelId من الأدمن الحالي (Logged-in User)
            // بنجيب الـ ID بتاع الادمن من التوكن
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new AuthModel { Message = "Unauthorized: Cannot determine admin user." };

            // بنروح الداتابيز نجيب بيانات الادمن ده عشان نعرف هو تبع فندق ايه
            var adminUser = await _userManager.FindByIdAsync(currentUserId);
            if (adminUser == null || adminUser.HotelId == null)
            {
                // ملحوظة: لو السوبر ادمن هو اللي بيسجل، وهو مش مربوط بفندق، ممكن نعديها أو نطلب HotelId
                // هنا هنفترض إن "اللي بيسجل" لازم يكون مدير فندق أو السوبر ادمن بيختار فندق
                // حسب طلبك: Extract from Admin's token. 
                // لو الادمن ملوش فندق، دي مشكلة بيزنس لازم تقررها، بس مبدئياً هنرجع ايرور
                return new AuthModel { Message = "Current admin is not assigned to a Hotel." };
            }

            // 3. رفع الصورة الشخصية (Profile Image)
            string? profileImgPath = null;
            if (model.ProfileImage != null)
            {
                profileImgPath = await SaveFileAsync(model.ProfileImage, "profile-images");
            }

            // 4. تجهيز بيانات الموظف
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
                HotelId = adminUser.HotelId, // ربطناه بنفس فندق الادمن
                IsActive = model.IsActive,
                ChangePasswordApprove = model.ChangePasswordApprove
            };

            // 5. حفظ الموظف في الداتابيز
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description},";
                return new AuthModel { Message = errors };
            }

            // 6. تعيين الرول (Role)
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return new AuthModel { Message = "Invalid Role selected." };
            }

            // ب) حماية إضافية: ممنوع حد يسجل موظف ويديله رول SuperAdmin من هنا
            // (الـ SuperAdmin بيتعمل بطريقة خاصة أو Seed بس)
            if (model.Role == "SuperAdmin")
            {
                return new AuthModel { Message = "Cannot assign SuperAdmin role to an employee." };
            }

            // ج) لو كله تمام، ضيفه للرول
            await _userManager.AddToRoleAsync(user, model.Role);


            // 7. رفع وحفظ المستندات (Documents)
            if (model.EmployeeDocs != null && model.EmployeeDocs.Count > 0)
            {
                foreach (var file in model.EmployeeDocs)
                {
                    // 1. نرفع الملف وناخد المسار
                    var docPath = await SaveFileAsync(file, "employee-docs");

                    // 2. نجهز الأوبجكت
                    var newDoc = new EmployeeDocument
                    {
                        FileName = file.FileName,
                        FileType = Path.GetExtension(file.FileName),
                        FilePath = docPath,
                        AppUserId = user.Id
                    };

                    // 3. نضيفه للـ UOW (من غير ما نعمل Save لسه)
                    await _unitOfWork.EmployeeDocuments.AddAsync(newDoc);
                }

                // 4. Save مرة واحدة بس في الآخر لكل الملفات (Performance Top 🚀)
                await _unitOfWork.CompleteAsync();
            }

            // 8. إرجاع النتيجة
            // مش محتاجين نرجع توكن، لأننا مش بنعمل لوجين للموظف، إحنا بس بنسجله
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

            // 2. التحقق من صحة اليوزر والباسورد
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Invalid Username or Password!" };

            // 3. (الجديد) التحقق من الفندق 🛑
            // لو اليوزر ليه فندق (مش سوبر أدمن)، والفندق اللي باعه غير فندقه المسجل -> اطرده
            if (user.HotelId != null && user.HotelId != model.HotelId)
            {
                return new AuthModel { Message = "Access Denied: You do not belong to this Hotel." };
            }

            // 4. التحقق من الحالة
            if (!user.IsActive)
                return new AuthModel { Message = "User is Disabled!" };

            // 5. إنشاء التوكن
            var token = await CreateJwtToken(user);

            // 6. إرجاع النتيجة
            return new AuthModel
            {
                IsAuthenticated = true,
                Token = token,
                Email = user.Email,
                Username = user.UserName,
                ExpiresOn = DateTime.Now.AddHours(_jwt.Value.DurationInHours),
                Roles = (List<string>)await _userManager.GetRolesAsync(user),
                Message = "Login Successful",

                ChangePasswordApprove = user.ChangePasswordApprove,
                HotelId = user.HotelId
            };
        }

        public async Task<AuthModel> ChangePasswordAsync(ChangePasswordDto model)
        {
            // 1. نجيب الـ User ID من التوكن للشخص اللي عامل لوجين حالياً
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new AuthModel { Message = "User not found or not logged in." };

            // 2. نجيب اليوزر من الداتابيز
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel { Message = "User not found." };

            // 3. تغيير الباسورد (Identity بتقوم بالواجب: بتتأكد من القديم وتهاش الجديد)
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description}, ";

                return new AuthModel { Message = errors };
            }

            // 4. (مهم جداً) تحديث الفلاج عشان ميتطلبش منه تغيير باسوورد تاني
            user.ChangePasswordApprove = false;

            // حفظ التعديل (بتاع الفلاج) في الداتابيز
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

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            // 1. نعرف مين اللي بينده الدالة دي حالياً
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 2. نجهز الكويري (هات اليوزرز ومعاهم الـ Status عشان الاسم)
            var query = _userManager.Users
                .Include(u => u.Status)
                .AsQueryable();

            // 3. تطبيق فلتر الفندق 🏨
            // لو المستخدم الحالي مربوط بفندق معين، هاتله الناس اللي معاه في نفس الفندق بس
            if (currentUser.HotelId != null)
            {
                query = query.Where(u => u.HotelId == currentUser.HotelId);
            }
            // (لو HotelId بـ null يبقى ده SuperAdmin، هنسيب الكويري مفتوحة تجيب كله)

            // تنفيذ الكويري
            var users = await query.ToListAsync();

            // 4. تحويل النتيجة لـ DTO
            var responseList = new List<UserResponseDto>();

            foreach (var user in users)
            {
                // بنجيب الرول لكل يوزر (ممكن يكون عنده أكتر من رول، هناخد الأولى كمثال)
                var roles = await _userManager.GetRolesAsync(user);

                responseList.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Status = user.Status?.Name ?? "Unknown", // لو مفيش حالة
                    Role = roles.FirstOrDefault() ?? "Employee", // أول رول تقابلنا
                    HotelId = user.HotelId
                });
            }

            return responseList;
        }

        public async Task<ApiResponse<UserDetailDto>> GetUserByIdAsync(string userId)
        {
            // 1. هات المستخدم الحالي (اللي بيطلب البيانات)
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 2. هات المستخدم المطلوب (Target User) مع بياناته الإضافية (مثل المستندات)
            var targetUser = await _userManager.Users
                .Include(u => u.Status)
                .Include(u => u.EmployeeDocs) // Assuming relation name is EmployeeDocs
                .FirstOrDefaultAsync(u => u.Id == userId);

            // 3. التحقق من الوجود
            if (targetUser == null)
                return new ApiResponse<UserDetailDto>("User not found.");

            // 4. التحقق من صلاحية الفندق (Security Check 👮‍♂️)
            // لو الطالب مدير فندق، والمطلوب في فندق تاني -> ارفض
            if (currentUser.HotelId != null && targetUser.HotelId != currentUser.HotelId)
            {
                return new ApiResponse<UserDetailDto>("Access Denied: You cannot view users from other hotels.");
            }

            // 5. تحويل البيانات لـ DTO
            var roles = await _userManager.GetRolesAsync(targetUser);

            var userDetail = new UserDetailDto
            {
                Id = targetUser.Id,
                FullName = targetUser.FullName,
                Username = targetUser.UserName,
                Email = targetUser.Email,
                PhoneNumber = targetUser.PhoneNumber,
                Status = targetUser.Status?.Name ?? "Unknown",
                Role = roles.FirstOrDefault() ?? "Employee",
                HotelId = targetUser.HotelId,

                // البيانات الإضافية
                NationalId = targetUser.NationalId,
                WorkNumber = targetUser.WorkNumber,
                Nationality = targetUser.Nationality,
                Gender = targetUser.Gender?.ToString(),
                DateOfBirth = targetUser.DateOfBirth,
                ProfileImagePath = targetUser.ProfileImagePath,
                DocumentPaths = targetUser.EmployeeDocs?.Select(d => d.FilePath).ToList() ?? new List<string>()
            };

            return new ApiResponse<UserDetailDto>(userDetail, "User details retrieved successfully");
        }


        public async Task<ApiResponse<string>> UpdateEmployeeAsync(UpdateEmployeeDto model)
        {
            // 1. هات المستخدم الحالي (اللي بيعمل التعديل)
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 2. هات الموظف اللي عايزين نعدله
            var userToUpdate = await _userManager.FindByIdAsync(model.Id);

            if (userToUpdate == null)
                return new ApiResponse<string>("User not found.");

            // 3. Security Check 👮‍♂️: ممنوع تعديل موظف خارج فندقك
            // (إلا لو أنت SuperAdmin والـ HotelId بتاعك null)
            if (currentUser.HotelId != null && userToUpdate.HotelId != currentUser.HotelId)
            {
                return new ApiResponse<string>("Access Denied: You cannot update users from other hotels.");
            }

            // 4. تحديث البيانات النصية (لو مبعوتة بقيمة)
            if (!string.IsNullOrEmpty(model.FullName)) userToUpdate.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.PhoneNumber)) userToUpdate.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrEmpty(model.NationalId)) userToUpdate.NationalId = model.NationalId;
            if (!string.IsNullOrEmpty(model.WorkNumber)) userToUpdate.WorkNumber = model.WorkNumber;
            if (!string.IsNullOrEmpty(model.Nationality)) userToUpdate.Nationality = model.Nationality;
            if (model.DateOfBirth.HasValue) userToUpdate.DateOfBirth = model.DateOfBirth.Value;
            if (model.IsActive.HasValue) userToUpdate.IsActive = model.IsActive.Value;

            // 5. تحديث الصورة الشخصية 🖼️
            if (model.ProfileImage != null)
            {
                // أ) لو فيه صورة قديمة، ممكن نمسحها (اختياري)
                if (!string.IsNullOrEmpty(userToUpdate.ProfileImagePath))
                {
                    // كود مسح الملف القديم (ممكن نعمله دالة مساعدة)
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, userToUpdate.ProfileImagePath.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                // ب) رفع الصورة الجديدة
                userToUpdate.ProfileImagePath = await SaveFileAsync(model.ProfileImage, "profile-images");
            }

            // 6. حفظ التعديلات الأساسية في الداتابيز
            var updateResult = await _userManager.UpdateAsync(userToUpdate);
            if (!updateResult.Succeeded)
                return new ApiResponse<string>(updateResult.Errors.Select(e => e.Description).ToList(), "Failed to update user.");

            // 7. تحديث الرول (Role) لو مبعوتة وتغيرت 🎭
            if (!string.IsNullOrEmpty(model.Role))
            {
                // نتأكد إن الرول موجودة وصالحة
                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                    // لو الرول الجديدة غير اللي معاه دلوقتي
                    if (!currentRoles.Contains(model.Role))
                    {
                        // شيل كل الرولات القديمة
                        await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                        // ضيف الجديدة
                        await _userManager.AddToRoleAsync(userToUpdate, model.Role);
                    }
                }
                else
                {
                    return new ApiResponse<string>("Invalid Role selected, other data updated successfully.");
                }
            }

            return new ApiResponse<string>(data: null, "User updated successfully");
        }


        // دالة Delete عادية جداً
        public async Task<ApiResponse<string>> DeleteUserAsync(string userId)
        {
            // 1. مين اللي بيعمل الحذف؟
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 2. مين اللي هيتحذف؟
            var userToDelete = await _userManager.FindByIdAsync(userId);

            if (userToDelete == null)
                return new ApiResponse<string>("User not found.");

            // 3. Security Check 👮‍♂️: ممنوع تحذف حد من فندق تاني
            if (currentUser.HotelId != null && userToDelete.HotelId != currentUser.HotelId)
            {
                return new ApiResponse<string>("Access Denied: You cannot delete users from other hotels.");
            }

            userToDelete.IsDeleted = true;
            userToDelete.DeletedAt = DateTime.UtcNow;
            userToDelete.DeletedBy = currentUserId; // بنسجل مين اللي مسحه


            var result = await _userManager.UpdateAsync(userToDelete);

            if (!result.Succeeded)
                return new ApiResponse<string>("Failed to delete user.");

            return new ApiResponse<string>(data: null, "User deleted successfully");
        }

        public async Task<ApiResponse<string>> RestoreUserAsync(string userId)
        {
            // 1. مين اللي بيعمل الاسترجاع؟
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // 2. البحث عن اليوزر الممسوح (لازم IgnoreQueryFilters ⚠️)
            var userToRestore = await _userManager.Users
                .IgnoreQueryFilters() // دي أهم حتة، عشان يشوف الممسوحين
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToRestore == null)
                return new ApiResponse<string>("User not found or not deleted.");

            // لو هو أصلاً مش ممسوح، ملوش لازمة نكمل
            if (!userToRestore.IsDeleted)
                return new ApiResponse<string>("User is not deleted.");

            // 3. Security Check 👮‍♂️
            if (currentUser.HotelId != null && userToRestore.HotelId != currentUser.HotelId)
            {
                return new ApiResponse<string>("Access Denied: You cannot restore users from other hotels.");
            }

            // 4. تصفير فلاجات الحذف (Restore)
            userToRestore.IsDeleted = false;
            userToRestore.DeletedAt = null;
            userToRestore.DeletedBy = null;

            // (اختياري) لو كنت خليته Inactive، ممكن ترجعه Active هنا أو تسيبه للمدير يفعله
            // userToRestore.IsActive = true;

            // 5. حفظ التعديل
            var result = await _userManager.UpdateAsync(userToRestore);

            if (!result.Succeeded)
                return new ApiResponse<string>("Failed to restore user.");

            return new ApiResponse<string>(data: null, "User restored successfully");
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
                expires: DateTime.Now.AddDays(_jwt.Value.DurationInHours),
                claims: userClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<string>> GetRolesAsync()
        {
            // بنجيب كل الرولات من الداتابيز
            var roles = await _roleManager.Roles
                // فلتر اختياري: مش عايزين نرجع "SuperAdmin" في الليسته عشان محدش يختاره بالغلط
                .Where(r => r.Name != "SuperAdmin")
                .Select(r => r.Name) // بناخد الاسم بس
                .ToListAsync();

            return roles;
        }
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            // لو WebRootPath بنل، بنستخدم المسار الحالي للمشروع + wwwroot
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
    }
}

