
using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.Models
{
    public class SafeUser
    {

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public int SafeId { get; set; }
        public virtual Safe? Safe { get; set; }

        [Display(Name = "Yetki Aktif mi?")]
        public bool IsActive { get; set; } = true; 



        public DateTime GrantedDate { get; set; } = DateTime.Now;
    }

}