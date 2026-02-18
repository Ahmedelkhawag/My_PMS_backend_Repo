using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Generates a daily guest report for the tourism police/authorities.
        /// </summary>
        /// <param name="businessDate">The specific business date to report on. Defaults to current business date if null.</param>
        /// <returns>Excel file as a byte array.</returns>
        Task<byte[]> GeneratePoliceReportAsync(DateTime? businessDate);
    }
}
