using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using PMS.Application.Interfaces.Services;
using PMS.Application.Settings;
using PMS.Domain.Entities;
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
        private readonly JWT _jwt;

        // بنحقن الحاجات اللي محتاجينها
        public AuthService(UserManager<AppUser> userManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthModel { Message = "Username is already taken!" };

            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,

                PhoneNumber = model.PhoneNumber,
                NationalId = model.NationalId,
                CountryID = model.CountryID,

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description}, ";

                return new AuthModel { Message = errors };
            }


            return new AuthModel
            {
                IsAuthenticated = true,
                Username = user.UserName,
                Email = user.Email,
                Message = "User registered successfully!",
                Roles = new List<string>(),
                Token = ""
            };
        }

        public async Task<AuthModel> LoginAsync(LoginDto model)
        {
            var user = await _userManager.Users
                .Include(u => u.Status) // <--- دي اللي هتملى الـ Status
                .SingleOrDefaultAsync(u => u.UserName == model.UserName);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Invalid Username or Password!" };

            if (user.Status.Name != "Active") return new AuthModel { Message = "User is blocked!" };

            var token = await CreateJwtToken(user);

            return new AuthModel
            {
                IsAuthenticated = true,
                Token = token,
                Email = user.Email,
                Username = user.UserName,
                ExpiresOn = DateTime.Now.AddDays(_jwt.DurationInDays),
                Roles = (List<string>)await _userManager.GetRolesAsync(user),
                Message = "Login Successful"
            };
        }

        private async Task<string> CreateJwtToken(AppUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                claims: userClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

