using KasaTakipSistemi.Models;
using System.Collections.Generic;

namespace KasaTakipSistemi.ViewModels
{
    public class DashboardViewModel
    {
        public string CurrentUserName { get; set; } = "Kullanıcı";
        public string SelectedSafeName { get; set; } = "Kasa Seçilmedi";
        public List<SafeBalanceViewModel> SafeBalances { get; set; } = new List<SafeBalanceViewModel>();
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new List<RecentTransactionViewModel>();
        public List<Safe> UserSafes { get; set; } = new List<Safe>(); 

        public decimal MonthlyIncomeTl { get; set; }
        public decimal MonthlyExpenseTl { get; set; }
        public decimal MonthlySummaryTl => MonthlyIncomeTl - MonthlyExpenseTl;
      
        public MonthlyChartDataViewModel? MonthlyChartData { get; set; } 
                                                                       


    }

    public class SafeBalanceViewModel
    {
        public string CurrencyName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class RecentTransactionViewModel
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencySymbol { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public bool IsIncome { get; set; }
        public string? PayeeOrPayer { get; set; }
    }
}