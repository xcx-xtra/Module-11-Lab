using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

namespace RubberDuckStore.Pages
{
    public class OrderFormModel : PageModel
    {
        [BindProperty]
        public Order? Order { get; set; }
        public Duck? Duck { get; set; }

public IActionResult OnGet(int duckId)
{
    Duck = GetDuckById(duckId);
    if (Duck == null)
    {
        return NotFound();
    }
    Order = new Order { DuckId = duckId };
    return Page();
}

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Duck = GetDuckById(Order.DuckId);
                return Page();
            }

            int orderId = SaveOrder(Order);

            return RedirectToPage("OrderConfirmation", new { orderId = orderId });
        }

        private Duck GetDuckById(int id)
        {
            using (var connection = new SqliteConnection("Data Source=RubberDucks.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Ducks WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Duck
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            ImageFileName = reader.GetString(4)
                        };
                    }
                }
            }
            return null;
        }

        private int SaveOrder(Order order)
        {
            using (var connection = new SqliteConnection("Data Source=RubberDucks.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Orders (DuckId, CustomerName, CustomerEmail, Quantity)
                    VALUES (@DuckId, @CustomerName, @CustomerEmail, @Quantity);
                    SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@DuckId", order.DuckId);
                command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                command.Parameters.AddWithValue("@CustomerEmail", order.CustomerEmail);
                command.Parameters.AddWithValue("@Quantity", order.Quantity);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public int DuckId { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required, EmailAddress]
        public string CustomerEmail { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}