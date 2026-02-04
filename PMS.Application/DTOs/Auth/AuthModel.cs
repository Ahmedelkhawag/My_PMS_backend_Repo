using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Auth
{
    public class AuthModel
    {
        public string Message { get; set; } // رسالة زي "تم التسجيل بنجاح"
        public bool IsAuthenticated { get; set; } // نجح ولا فشل
        public string Username { get; set; }
        public string Email { get; set; }
        public bool ChangePasswordApprove { get; set; } // عشان الفرونت يعرف يوجهه
        public int? HotelId { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; } // الـ JWT المهمة
        public DateTime ExpiresOn { get; set; }
    }
}
