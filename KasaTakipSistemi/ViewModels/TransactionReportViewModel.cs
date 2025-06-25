
using System;
using System.ComponentModel.DataAnnotations;
using KasaTakipSistemi.Models; 

namespace KasaTakipSistemi.ViewModels
{
    public class TransactionReportViewModel
    {
        [Display(Name = "Tarih")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Cari/Personel")]
        public string? PayeeOrPayer { get; set; }

        [Display(Name = "Gelir")]
        public decimal? IncomeAmount { get; set; } 

        [Display(Name = "Gider")]
        public decimal? ExpenseAmount { get; set; }
        [Display(Name = "Para Birimi")]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Display(Name = "Kasa")]
        public string SafeName { get; set; } = string.Empty;

        public TransactionType OriginalTransactionType { get; set; }
    }
}