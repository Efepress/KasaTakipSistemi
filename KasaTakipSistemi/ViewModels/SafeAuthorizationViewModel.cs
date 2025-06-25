
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; 
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace KasaTakipSistemi.ViewModels
{
 
    public class SafeAuthorizationViewModel
    {
       
        public string UserId { get; set; } = string.Empty;
        public int SafeId { get; set; }

        [Display(Name = "Yetkili Kullanıcı Adı Soyadı")]
        public string UserFullName { get; set; } = string.Empty;

        [Display(Name = "Kullanıcı E-posta")]
        public string UserEmail { get; set; } = string.Empty;

        [Display(Name = "Yetkili Olunan Kasa")]
        public string SafeName { get; set; } = string.Empty;

        [Display(Name = "Yetki Durumu")]
        public bool IsActive { get; set; }

    
        public string StatusText => IsActive ? "Aktif" : "Pasif";
        public string StatusCssClass => IsActive ? "badge bg-success" : "badge bg-danger";
        public string ToggleActionText => IsActive ? "Pasifleştir" : "Aktifleştir";
        public string ToggleActionIcon => IsActive ? "fas fa-ban" : "fas fa-check";
        public string ToggleButtonCssClass => IsActive ? "btn btn-sm btn-warning" : "btn btn-sm btn-success";
    }


}