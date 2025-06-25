
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using System.Collections.Generic; 
namespace KasaTakipSistemi.Models
{
    public enum AccountType
    {
        [Display(Name = "Müşteri")]
        Musteri = 1,
        [Display(Name = "Satıcı")] 
        Satici = 2,
        [Display(Name = "Diğer")]
        Diger = 3
    }

    public class CurrentAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cari Hesap Adı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Cari Hesap Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Cari Türü")]
        public AccountType AccountType { get; set; } = AccountType.Musteri;

        [StringLength(15)]
        [Display(Name = "Vergi Numarası")]
        public string? TaxNumber { get; set; } 

        [StringLength(11)]
        [Display(Name = "TC Kimlik Numarası")]
        public string? IdentificationNumber { get; set; } 

        [StringLength(150)]
        [Display(Name = "Adres")]
        public string? Address { get; set; } 

        [StringLength(15)]
        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; } 

        [StringLength(100)]
        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; } 

        [StringLength(250)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; } 

        
        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}