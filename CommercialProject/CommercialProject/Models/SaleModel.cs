using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommercialProject.Models
{
    public class SaleModel
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Price { get; set; }
    }
}
