using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Common
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }       // الداتا بتاعة الصفحة دي
        public int TotalCount { get; set; }     // العدد الكلي في الداتابيز
        public int PageNumber { get; set; }     // رقم الصفحة الحالية
        public int PageSize { get; set; }       // حجم الصفحة

        // خاصية محسوبة (عدد الصفحات الكلي)
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public PagedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
