
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KasaTakipSistemi.Models
{
    public class Bank
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Banka Adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Banka Adı")]
        public string Name { get; set; } = string.Empty; 

      
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    }
}