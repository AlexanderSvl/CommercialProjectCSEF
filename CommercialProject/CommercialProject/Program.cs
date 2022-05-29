using Aspose.Pdf;
using Aspose.Pdf.Text;
using CommercialProject.Models;
using System;
using System.Globalization;

namespace CommercialProject
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommercialProjectContext context = new CommercialProjectContext();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Statistics:");
            ShowStatistics(context);
            Console.ResetColor();
            Console.WriteLine();

            options:

            Console.WriteLine("1 - Add a product (Make a delivery OR increase quantity of existing product)" + Environment.NewLine + "2 - Sell a product" + Environment.NewLine + "3 - Quit");
            Console.WriteLine();

            Console.Write("Option: ");
            int option = int.Parse(Console.ReadLine());
            Console.WriteLine();

            if(option == 1)
            {
                AddProduct(context);
            }
            else if(option == 2)
            {
                SellProduct(context);
            }
            else if(option == 3)
            {
                Environment.Exit(0);
            }
            else
            {
                PrintErrorMessage($"There is no option {option}! Please, enter one of the following options:");
                Console.WriteLine();
                goto options;
            }
        }

        public static void PrintErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static string ShowStatistics(CommercialProjectContext context)
        {
            decimal totalSales = 0;

            if (context.Sales.Count() == 0)
            {
                PrintErrorMessage("No sales to take data from!");
                return "";
            }

            context.Sales
                .ToList()
                .ForEach(sale => totalSales += sale.Price);

            context.Inventory
                .ToList()
                .ForEach(item => totalSales -= item.DeliveryPrice);

            Console.WriteLine($"Total sales - {context.Sales.Count()}  | - | - | - | Total profit - {decimal.Round(totalSales, 2, MidpointRounding.AwayFromZero)}");
            return "";
        }

        public static void AddProduct(CommercialProjectContext context)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter product name: ");
            Console.ResetColor();
            string productName = Console.ReadLine();

            if(productName == "" || productName == null)
            {
                Console.WriteLine();
                PrintErrorMessage("Product name cannot be empty!");
                Console.WriteLine();

                AddProduct(context);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Enter product quantity: ");
            Console.ResetColor();
            int quantity = int.Parse(Console.ReadLine());

            if (quantity == null)
            {
                PrintErrorMessage("You must enter product quantity!");
                AddProduct(context);
            }

            var productCheck = context.Products.ToList().Find(x => x.Name == productName);

            if (productCheck != null)
            {
                context.Inventory
                    .Where(inv => inv.ProductId == productCheck.Id)
                    .ToList()
                    .FirstOrDefault()
                    .Quantity += quantity;

                context.SaveChanges();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.Write("Enter product delivery price: ");
                Console.ResetColor();
                decimal deliveryPrice = decimal.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.Write("Enter product category: ");
                Console.ResetColor();
                string productCategory = Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.Write("Enter product description: ");
                Console.ResetColor();
                string productDescription = Console.ReadLine();

                ProductModel product =
                new ProductModel
                {
                    Name = productName,
                    Category = productCategory,
                    Description = productDescription
                };

                context.Products
                    .Add(product);

                context.Inventory
                    .Add
                    (
                        new ProductAvailabilityInfo
                        {
                            Product = product,
                            Deliveries = new List<string>(),
                            DeliveryPrice = deliveryPrice,
                            Quantity = quantity,
                        }
                    );

                context.SaveChanges();

                DeliveryProtocolPDF(product, quantity, context);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Product added.");
            Console.ResetColor();
        }

        public static void SellProduct(CommercialProjectContext context)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter product name: ");
            Console.ResetColor();
            string productName = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Enter product quantity: ");
            Console.ResetColor();
            int quantity = int.Parse(Console.ReadLine());
            Console.WriteLine();

            var productToSell = context.Products
                .Where(x => x.Name == productName)
                .First();

            if (productToSell == null)
            {
                PrintErrorMessage($"There is no product with the name \"{productName}\" in our inventory!");
            }
            else
            {
                var product = context.Inventory
                    .Where(x => x.ProductId == productToSell.Id)
                    .First();

                if (product.Quantity < quantity)
                {
                    PrintErrorMessage($"There is not enough quanity of {productName}");
                }
                else
                {
                    decimal deliveryPrice = product.DeliveryPrice;
                    decimal sellPrice = CalculateProfit(deliveryPrice);

                    SaleModel sale = new SaleModel();

                    for (int i = 0; i < quantity; i++)
                    {
                         sale =
                            new SaleModel
                            {
                                ProductId = product.ProductId,
                                Product = productToSell,
                                Price = sellPrice,
                                SaleDate = DateTime.Now
                            };

                        context.Sales
                            .Add(sale);

                        context.SaveChanges();
                    }

                    SaleProtocolPDF(sale, quantity, context);

                    product.Deliveries = new List<string>();

                    product.Deliveries
                        .Add($"Date: {DateTime.Now} | Price: {sellPrice}");

                    product.Quantity -= quantity;
                    context.SaveChanges();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Product \"{productName}\" was sold. Profit is ${decimal.Round(((sellPrice - deliveryPrice) * quantity), 2, MidpointRounding.AwayFromZero)}");
                    Console.ResetColor();
                }
            }
        }

        public static void DeliveryProtocolPDF(ProductModel productModel, int quantity, CommercialProjectContext context)
        {
            ProductAvailabilityInfo delivery = context.Inventory
                .FirstOrDefault(x => x.Product.Id == productModel.Id);

            Document documentToExport = new Document();
            Page page = documentToExport.Pages.Add();

            TextFragment deliveryIdFragment =
                new TextFragment
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"Delivery ID: {delivery.Id}",
                    Margin = new MarginInfo(10f, 10f, 10f, 10f)
                };

            TextFragment productFragment =
                new TextFragment
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"Product: {productModel.Name}",
                    Margin = new MarginInfo(10f, 10f, 10f, 10f)
                };

            TextFragment quantityFragment =
               new TextFragment
               {
                   HorizontalAlignment = HorizontalAlignment.Center,
                   Text = $"Quantity: {quantity}",
                   Margin = new MarginInfo(10f, 10f, 10f, 10f)
               };

            TextFragment priceFragment =
              new TextFragment
              {
                  HorizontalAlignment = HorizontalAlignment.Center,
                  Text = $"Delivery Price: {decimal.Round(delivery.DeliveryPrice * quantity, 2, MidpointRounding.AwayFromZero)}",
                  Margin = new MarginInfo(10f, 10f, 10f, 10f)
              };

            TextFragment dateFragment =
             new TextFragment
             {
                 HorizontalAlignment = HorizontalAlignment.Center,
                 Text = $"Delivery Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                 Margin = new MarginInfo(10f, 10f, 10f, 10f)
             };

            page.Paragraphs.Add(deliveryIdFragment);
            page.Paragraphs.Add(productFragment);
            page.Paragraphs.Add(quantityFragment);
            page.Paragraphs.Add(priceFragment);
            page.Paragraphs.Add(dateFragment);

            documentToExport.Save($"../../../Deliveries/delivery{delivery.Id}.pdf");
        }

        public static void SaleProtocolPDF(SaleModel sale, int quantity, CommercialProjectContext context)
        {
            int saleID = context.Sales
                .FirstOrDefault(x => x.SaleDate == sale.SaleDate)
                .Id;

            Document documentToExport = new Document();
            Page page = documentToExport.Pages.Add();

            TextFragment saleIdFragment = 
                new TextFragment
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"Sale ID: {saleID}",
                    Margin = new MarginInfo(10f, 10f, 10f, 10f)
                };

            TextFragment productFragment =
                new TextFragment
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"Product: {sale.Product.Name}",
                    Margin = new MarginInfo(10f, 10f, 10f, 10f)
                };

            TextFragment quantityFragment =
               new TextFragment
               {
                   HorizontalAlignment = HorizontalAlignment.Center,
                   Text = $"Quantity: {quantity}",
                   Margin = new MarginInfo(10f, 10f, 10f, 10f)
               };

            TextFragment priceFragment =
              new TextFragment
              {
                  HorizontalAlignment = HorizontalAlignment.Center,
                  Text = $"Price: {decimal.Round(sale.Price * quantity, 2, MidpointRounding.AwayFromZero)}",
                  Margin = new MarginInfo(10f, 10f, 10f, 10f)
              };

            TextFragment dateFragment =
             new TextFragment
             {
                 HorizontalAlignment = HorizontalAlignment.Center,
                 Text = $"Sale Date: {sale.SaleDate.ToString(CultureInfo.InvariantCulture)}",
                 Margin = new MarginInfo(10f, 10f, 10f, 10f)
             };

            page.Paragraphs.Add(saleIdFragment);
            page.Paragraphs.Add(productFragment);
            page.Paragraphs.Add(quantityFragment);
            page.Paragraphs.Add(priceFragment);
            page.Paragraphs.Add(dateFragment);

            documentToExport.Save($"../../../Sales/sale{saleID}.pdf");
        }

        public static decimal CalculateProfit(decimal deliveryPrice)
        {
            return deliveryPrice + (decimal)0.25 * deliveryPrice;
        }
    }
}