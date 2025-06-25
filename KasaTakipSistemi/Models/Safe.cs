using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; 

namespace KasaTakipSistemi.Models
{
    public class Safe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Kasa Adı")]
        public string Name { get; set; } = string.Empty; 
        [Required]
        public string UserId { get; set; } = string.Empty; 
        public virtual ApplicationUser? User { get; set; } 

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<SafeUser> AuthorizedUsers { get; set; } = new List<SafeUser>();
    }
}