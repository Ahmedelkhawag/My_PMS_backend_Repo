using System.Collections.Generic;

namespace PMS.Application.DTOs.BackOffice
{
    public class FinancialReportLineDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool IsGroup { get; set; }
        public decimal Balance { get; set; }
        public List<FinancialReportLineDto> ChildLines { get; set; } = new List<FinancialReportLineDto>();
    }

    public class PnLReportDto
    {
        public List<FinancialReportLineDto> Revenues { get; set; } = new List<FinancialReportLineDto>();
        public List<FinancialReportLineDto> Expenses { get; set; } = new List<FinancialReportLineDto>();
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetProfitLoss { get; set; }
    }

    public class BalanceSheetDto
    {
        public List<FinancialReportLineDto> Assets { get; set; } = new List<FinancialReportLineDto>();
        public List<FinancialReportLineDto> Liabilities { get; set; } = new List<FinancialReportLineDto>();
        public List<FinancialReportLineDto> Equities { get; set; } = new List<FinancialReportLineDto>();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
    }
}
