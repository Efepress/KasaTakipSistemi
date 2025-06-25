
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; 
using System.Collections.Generic;     

namespace KasaTakipSistemi.ViewModels
{
    public enum ReportTransactionType
    {
        [Display(Name = "Tümü")]
        Tumu = 0,
        [Display(Name = "Gelirler")]
        Gelirler = 1,
        [Display(Name = "Giderler")]
        Giderler = 2
    }

    public class ReportFilterViewModel
    {
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now.Date.AddMonths(-1); 

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.Date;

        [Display(Name = "Hesap Türü / Cari")]
        public string? AccountId { get; set; } 
                                          

        [Display(Name = "İşlem Türü")]
        public ReportTransactionType TransactionType { get; set; } = ReportTransactionType.Tumu;


        
        public List<SelectListItem> AccountList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TransactionTypeList { get; set; } = new List<SelectListItem>();

      
        public List<TransactionReportViewModel> ReportResults { get; set; } = new List<TransactionReportViewModel>();
        public bool HasResults { get; set; } = false;

  
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetTotal => TotalIncome - TotalExpense;
        public string? SelectedCurrencySymbolForTotals { get; set; }
    }
}