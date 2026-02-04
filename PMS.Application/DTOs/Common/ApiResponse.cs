using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }

        // Constructors لتسهيل الاستخدام

        public ApiResponse() { }

        // حالة النجاح
        public ApiResponse(T data, string message = null)
        {
            Succeeded = true;
            Message = message ?? "Operation successful";
            Data = data;
            Errors = null;
        }

        // حالة الفشل
        public ApiResponse(string message)
        {
            Succeeded = false;
            Message = message;
            Errors = new List<string> { message };
        }

        // حالة الفشل مع قائمة أخطاء (للـ Validation)
        public ApiResponse(List<string> errors, string message = "Validation failed")
        {
            Succeeded = false;
            Message = message;
            Errors = errors;
        }
    }
}

