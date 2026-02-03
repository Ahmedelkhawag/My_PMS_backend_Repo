using PMS.Application.DTOs;
using PMS.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterDto model);

        Task<AuthModel> LoginAsync(LoginDto model);
    }
}
