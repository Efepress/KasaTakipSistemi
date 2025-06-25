using Microsoft.AspNetCore.Mvc.Rendering; 
using System.Collections.Generic;      
using System.ComponentModel.DataAnnotations;  

namespace KasaTakipSistemi.ViewModels
{
    public class AssignSafeAuthorizationViewModel
    {
        [Required(ErrorMessage = "Kullanıcı seçimi zorunludur.")]
        [Display(Name = "Yetkilendirilecek Kullanıcı")]
        public string SelectedUserId { get; set; } = string.Empty;
        public List<SelectListItem> UserList { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Kasa seçimi zorunludur.")]
        [Display(Name = "Yetki Verilecek Kasa")]
        public int SelectedSafeId { get; set; }
        public List<SelectListItem> SafeList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Yetkiyi Aktif Et")]
        public bool IsActive { get; set; } = true;
    }
}