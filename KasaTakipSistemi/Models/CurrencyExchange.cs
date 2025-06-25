
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasaTakipSistemi.Models
{
    public class CurrencyExchange
    {
        public int Id { get; set; }

        [Display(Name = "Bozdurulan Hesap Türü")]
        public AccountSourceType SoldAccountSource { get; set; } = AccountSourceType.Nakit; 

        [Display(Name = "Bozdurulan Banka Hesabı")]
        public int? SoldBankAccountId { get; set; } 
        

        [Required(ErrorMessage = "Bozdurulan Para Birimi zorunludur.")]
        [Display(Name = "Bozdurulan Para Birimi")]
        public int SoldCurrencyId { get; set; }
        public virtual Currency? SoldCurrency { get; set; }

        [Required(ErrorMessage = "Bozdurulan Miktar zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Bozdurulan Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal SoldAmount { get; set; }

        [Display(Name = "Alınan Hesap Türü")]
        public AccountSourceType BoughtAccountSource { get; set; } = AccountSourceType.Nakit; 

        [Display(Name = "Alınan Banka Hesabı")]
        public int? BoughtBankAccountId { get; set; } 
        

        [Required(ErrorMessage = "Alınan Para Birimi zorunludur.")]
        [Display(Name = "Alınan Para Birimi")]
        public int BoughtCurrencyId { get; set; }
        public virtual Currency? BoughtCurrency { get; set; }

        [Required(ErrorMessage = "Alınan Miktar zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Alınan Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal BoughtAmount { get; set; }

        [Display(Name = "İşlem Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime ExchangeDate { get; set; } = DateTime.Now;

        [Display(Name = "Döviz Bürosu / Cari")]
        public int? CurrentAccountId { get; set; } 
        public virtual CurrentAccount? CurrentAccount { get; set; }

        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

       
        [Required]
        public int ExpenseTransactionId { get; set; }
        public virtual Transaction? ExpenseTransaction { get; set; }

       
        [Required]
        public int IncomeTransactionId { get; set; }
        public virtual Transaction? IncomeTransaction { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

      
        [Required]
        [Display(Name = "İşlemin Yapıldığı Ana Kasa")]
        public int MainSafeId { get; set; }
        public virtual Safe? MainSafe { get; set; }
    }

    public enum AccountSourceType
    {
        [Display(Name = "Nakit (Ana Kasa)")]
        Nakit = 1,
        [Display(Name = "Banka Hesabı")]
        Banka = 2
    }
}