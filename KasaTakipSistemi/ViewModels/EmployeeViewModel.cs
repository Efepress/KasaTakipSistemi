
using KasaTakipSistemi.Models;
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Adı Soyadı")]
        public string FullName { get; set; } = string.Empty;
        [Display(Name = "Maaş Bilgisi")]
        public string MaasBilgiStr { get; set; } = string.Empty; 
        [Display(Name = "Bağlı Kasa")]
        public string KasaAd { get; set; } = string.Empty; 
        public bool IsActive { get; set; }
    }
}