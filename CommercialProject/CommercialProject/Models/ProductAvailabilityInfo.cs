using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommercialProject.Models
{
    public class ProductAvailabilityInfo
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }
        [NotMapped]
        public List<string> Deliveries { get; set; }
        public decimal DeliveryPrice { get; set; }
        public int Quantity { get; set; }
    }
}
