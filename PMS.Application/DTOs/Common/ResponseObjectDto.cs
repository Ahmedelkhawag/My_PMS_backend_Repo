using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Common
{
    public class ResponseObjectDto<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public static ResponseObjectDto<T> Success(T data, string message = "Success", int statusCode = 200)
        {
            return new ResponseObjectDto<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        // دالة جاهزة في حالة الفشل
        public static ResponseObjectDto<T> Failure(string message, int statusCode = 400)
        {
            return new ResponseObjectDto<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default, // هيكون null
                StatusCode = statusCode
            };
        }
    }
}
