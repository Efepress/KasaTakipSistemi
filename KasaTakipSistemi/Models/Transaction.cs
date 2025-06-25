using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; 

namespace KasaTakipSistemi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int SafeId { get; set; }
        public virtual Safe? Safe { get; set; } 

        [Required]
        [Display(Name = "İşlem Türü")]
        public TransactionType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }

        public int CurrencyId { get; set; }
        public virtual Currency? Currency { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty; 

        [Display(Name = "İşlem Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Cari Hesap (Kişi/Firma)")]
        public string? PayeeOrPayer { get; set; } 

        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; } 
    }
}