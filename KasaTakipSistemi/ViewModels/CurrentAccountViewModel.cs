
using KasaTakipSistemi.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.ViewModels
{
    public class CurrentAccountViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Cari Hesap Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Cari Türü")]
        public string AccountTypeStr { get; set; } = string.Empty; 

   
        public List<AccountBalanceViewModel> Balances { get; set; } = new List<AccountBalanceViewModel>();

     
        public string AllBalancesSummaryHtml { get; set; } = string.Empty;

       
        public string? TaxNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class AccountBalanceViewModel
    {
        public string CurrencyName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AmountDisplay => $"{Amount:N2} {CurrencySymbol}";
        public string CssClass 
        {
            get
            {
                if (Amount > 0) return "text-success"; 
                if (Amount < 0) return "text-danger";  
                return "text-dark";                  
            }
        }
    }
}