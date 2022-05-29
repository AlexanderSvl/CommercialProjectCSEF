using CommercialProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommercialProject
{
    public class CommercialProjectContext : DbContext
    {
        public CommercialProjectContext()
        {
        }

        public CommercialProjectContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ProductModel> Products { get; set; }

        public DbSet<ProductAvailabilityInfo> Inventory { get; set; }

        public DbSet<SaleModel> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CommercialProjectDatabase;Integrated Security=True;");
        }
    }
}
