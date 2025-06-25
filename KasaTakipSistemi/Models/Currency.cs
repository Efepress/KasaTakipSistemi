using System.ComponentModel.DataAnnotations;

namespace KasaTakipSistemi.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Para Birimi Adı")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        [Display(Name = "Sembol")]
        public string Symbol { get; set; } = string.Empty; 
    }
}