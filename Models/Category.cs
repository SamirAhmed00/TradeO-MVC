using System.ComponentModel.DataAnnotations;

namespace TradeO.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name field is required!")]
        [StringLength(maximumLength: 100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters!")]
        public string Name { get; set; } = string.Empty;
        
        public int DisplayOrder { get; set; }
    }
}
