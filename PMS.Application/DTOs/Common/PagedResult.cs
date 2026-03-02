using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs.Common
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }       
        public int TotalCount { get; set; }     
        public int PageNumber { get; set; }     
        public int PageSize { get; set; }       

        
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
