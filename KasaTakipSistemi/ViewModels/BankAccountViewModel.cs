
using KasaTakipSistemi.Models;
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.ViewModels
{
    public class BankAccountViewModel 
    {
        public int Id { get; set; }

        [Display(Name = "Hesap Adı")]
        public string AccountName { get; set; } = string.Empty;

        [Display(Name = "Banka Adı")]
        public string BankName { get; set; } = string.Empty;

        [Display(Name = "Hesap Türü")]
        public string AccountTypeName { get; set; } = string.Empty;

        [Display(Name = "Para Birimi")]
        public string CurrencyName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;

        public string? Iban { get; set; }



    }

    public class BankAccountCreateEditViewModel 
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hesap Adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Hesap Adı / Tanımı")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Banka seçimi zorunludur.")]
        [Display(Name = "Banka")]
        public int BankId { get; set; }

        [Display(Name = "Hesap Türü")]
        public BankAccountType AccountType { get; set; } = BankAccountType.Vadesiz;

        [Required(ErrorMessage = "Para Birimi zorunludur.")]
        [Display(Name = "Para Birimi")]
        public int CurrencyId { get; set; }

        [StringLength(50)]
        [Display(Name = "Şube Kodu")]
        public string? BranchCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Hesap Numarası")]
        public string? AccountNumber { get; set; }

        [StringLength(34)]
        [Display(Name = "IBAN")]
        public string? Iban { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [StringLength(250)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }


    }
}