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
    }
}
