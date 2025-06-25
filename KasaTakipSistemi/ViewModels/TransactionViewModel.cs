using KasaTakipSistemi.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Kasa seçimi zorunludur.")]
        [Display(Name = "Kasa")]
        public int SafeId { get; set; }

        [Required(ErrorMessage = "İşlem türü zorunludur.")]
        [Display(Name = "İşlem Türü")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Para birimi seçimi zorunludur.")]
        [Display(Name = "Para Birimi")]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "İşlem tarihi zorunludur.")]
        [Display(Name = "İşlem Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Cari Hesap (Kişi/Firma)")]
        public string? PayeeOrPayer { get; set; }
    }
}