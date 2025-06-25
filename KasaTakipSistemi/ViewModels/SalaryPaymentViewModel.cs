
using System;
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.ViewModels
{
    public class SalaryPaymentViewModel
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Personel seçimi zorunludur.")]
        [Display(Name = "Personel")]
        public int EmployeeId { get; set; }
        public string? EmployeeFullName { get; set; } 

        [Required(ErrorMessage = "Ödeme tarihi zorunludur.")]
        [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now.Date;

        [Required(ErrorMessage = "Ödenen miktar zorunludur.")]
        [Display(Name = "Ödenen Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [Display(Name = "Para Birimi")]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Ödemenin yapılacağı kasa zorunludur.")]
        [Display(Name = "Ödeme Kasası")]
        public int SafeId { get; set; }

        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }
}