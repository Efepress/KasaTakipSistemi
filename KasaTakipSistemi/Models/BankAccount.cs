
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasaTakipSistemi.Models
{
    public enum BankAccountType
    {
        [Display(Name = "Vadesiz Hesap")]
        Vadesiz = 1,
        [Display(Name = "Vadeli Hesap")]
        Vadeli = 2,
        [Display(Name = "Kredi Kartı")]
        KrediKarti = 3,
        [Display(Name = "POS Hesabı")]
        Pos = 4,
        [Display(Name = "Diğer")]
        Diger = 5
    }

    public class BankAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hesap Adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Hesap Adı / Tanımı")]
        public string AccountName { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Banka seçimi zorunludur.")]
        [Display(Name = "Banka")]
        public int BankId { get; set; }
        public virtual Bank? Bank { get; set; }

        [Display(Name = "Hesap Türü")]
        public BankAccountType AccountType { get; set; } = BankAccountType.Vadesiz;

        [Required(ErrorMessage = "Para Birimi zorunludur.")]
        [Display(Name = "Para Birimi")]
        public int CurrencyId { get; set; }
        public virtual Currency? Currency { get; set; }

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

        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

       
        [NotMapped]
        public string TurAd => AccountType.ToString();
        [NotMapped]
        public string ParaBirimiAd => Currency?.Name ?? "";
        [NotMapped]
        public string BankaAd => Bank?.Name ?? "";
    }
}