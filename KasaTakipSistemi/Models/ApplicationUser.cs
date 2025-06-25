using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations; 
using System.Collections.Generic;      

namespace KasaTakipSistemi.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; } 

       
        public virtual ICollection<Safe> Safes { get; set; } = new List<Safe>();
        public virtual ICollection<SafeUser> AuthorizedSafes { get; set; } = new List<SafeUser>();
    }
}