
using System;
using System.ComponentModel.DataAnnotations;
using KasaTakipSistemi.Models; 

namespace KasaTakipSistemi.ViewModels
{
    public class CurrencyExchangeViewModel
    {

        [Required(ErrorMessage = "Bozdurulacak Hesap Türü seçimi zorunludur.")]
        [Display(Name = "Bozdurulan Hesap")]
        public AccountSourceType SoldAccountSource { get; set; } = AccountSourceType.Nakit;

       

        [Required(ErrorMessage = "Bozdurulan Para Birimi zorunludur.")]
        [Display(Name = "Bozdurulan Para Birimi")]
        public int SoldCurrencyId { get; set; }

        [Required(ErrorMessage = "Bozdurulan Miktar zorunludur.")]
        [Display(Name = "Bozdurulan Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal SoldAmount { get; set; }

       
        [Required(ErrorMessage = "Alınacak Hesap Türü seçimi zorunludur.")]
        [Display(Name = "Alınan Hesap")]
        public AccountSourceType BoughtAccountSource { get; set; } = AccountSourceType.Nakit;

       

        [Required(ErrorMessage = "Alınan Para Birimi zorunludur.")]
        [Display(Name = "Alınan Para Birimi")]
        public int BoughtCurrencyId { get; set; }

        [Required(ErrorMessage = "Alınan Miktar zorunludur.")]
        [Display(Name = "Alınan Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal BoughtAmount { get; set; }


        [Display(Name = "İşlem Tarihi")]
        [DataType(DataType.DateTime)] 
        public DateTime ExchangeDate { get; set; } = DateTime.Now;

        [Display(Name = "Döviz Bürosu / Cari")]
        public int? CurrentAccountId { get; set; } 

        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        public int SelectedSafeId { get; set; }
    }
}