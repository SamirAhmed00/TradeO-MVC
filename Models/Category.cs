using System.ComponentModel.DataAnnotations;

namespace TradeO.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
