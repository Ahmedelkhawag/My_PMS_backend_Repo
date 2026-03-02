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

        

        public ApiResponse() { }

        
        public ApiResponse(T data, string message = null)
        {
            Succeeded = true;
            Message = message ?? "Operation successful";
            Data = data;
            Errors = null;
        }

        
        public ApiResponse(string message)
        {
            Succeeded = false;
            Message = message;
            Errors = new List<string> { message };
        }

        
        public ApiResponse(List<string> errors, string message = "Validation failed")
        {
            Succeeded = false;
            Message = message;
            Errors = errors;
        }
    }
}

