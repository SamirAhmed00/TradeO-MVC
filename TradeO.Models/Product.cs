using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeO.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name Is Required")]
        [StringLength(100,ErrorMessage = "Name can't be longer than 100 characters")]
        [MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
        public string Name { get; set; }

        [StringLength(maximumLength: 5000, ErrorMessage = "Description can't exceed 5000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Seller Name Is Required")]
        [StringLength(50, ErrorMessage = "Seller name can't be longer than 50 characters")]
        public string Seller { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 1000000, ErrorMessage = "Discount price must be positive")]
        public decimal? DiscountPrice { get; set; }

        //[Url(ErrorMessage = "Invalid image URL")]
        [StringLength(500, ErrorMessage = "Image URL too long")]
        public string ImageUrl { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
    }
}
