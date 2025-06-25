
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; 

namespace KasaTakipSistemi.Models
{
    public enum SalaryPeriod
    {
        [Display(Name = "Aylık")]
        Aylik = 1,
        [Display(Name = "Haftalık")]
        Haftalik = 2,
        [Display(Name = "Günlük")] 
        Gunluk = 3
    }

    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Personel Adı Soyadı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Adı Soyadı")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Pozisyon")]
        public string? Position { get; set; } 

        [Required(ErrorMessage = "Maaş miktarı zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Maaş Miktarı")]
        [Range(0, double.MaxValue, ErrorMessage = "Maaş miktarı negatif olamaz.")]
        public decimal SalaryAmount { get; set; }

        [Required(ErrorMessage = "Maaş para birimi zorunludur.")]
        [Display(Name = "Maaş Para Birimi")]
        public int SalaryCurrencyId { get; set; }
        public virtual Currency? SalaryCurrency { get; set; }

        [Display(Name = "Maaş Periyodu")]
        public SalaryPeriod SalaryPeriod { get; set; } = SalaryPeriod.Aylik;

        [Display(Name = "İşe Başlama Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; } 

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [StringLength(250)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; } 

        [Display(Name = "Bağlı Olduğu Kasa")]
        public int? DefaultSafeId { get; set; }
        public virtual Safe? DefaultSafe { get; set; }



        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<SalaryPayment> SalaryPayments { get; set; } = new List<SalaryPayment>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

      
        [NotMapped] 
        public string MaasBilgiStr => $"{SalaryPeriod} {SalaryAmount:N2} {SalaryCurrency?.Symbol}";

        [NotMapped]
        public string KasaAd => DefaultSafe?.Name ?? "-";
    }
}