
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasaTakipSistemi.Models
{
    public class SalaryPayment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Personel")]
        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now.Date;

        [Required(ErrorMessage = "Ödenen Miktar zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Ödenen Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödenen miktar 0'dan büyük olmalıdır.")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Ödeme Para Birimi zorunludur.")]
        [Display(Name = "Ödeme Para Birimi")]
        public int CurrencyId { get; set; }
        public virtual Currency? Currency { get; set; }

        [Required(ErrorMessage = "Ödemenin Yapıldığı Kasa zorunludur.")]
        [Display(Name = "Ödemenin Yapıldığı Kasa")]
        public int SafeId { get; set; }
        public virtual Safe? Safe { get; set; }

        [StringLength(250)]
        [Display(Name = "Açıklama / Not")]
        public string? Description { get; set; }


        public int? TransactionId { get; set; }
        public virtual Transaction? Transaction { get; set; }


        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}