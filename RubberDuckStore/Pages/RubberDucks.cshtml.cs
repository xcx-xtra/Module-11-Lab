using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace RubberDuckStore.Pages
{
    // This is a PageModel for a Razor Page that handles displaying rubber duck products
    public class RubberDucksModel : PageModel
    {
        // Property that will store the selected duck ID from form submissions
        [BindProperty]
        public int SelectedDuckId { get; set; }

        // List that will hold all ducks for the dropdown selection
        public List<SelectListItem> DuckList { get; set; } = new List<SelectListItem>();
        
        // Property that will store the currently selected duck object
        public Duck? SelectedDuck { get; set; }

        // Handles HTTP GET requests to the page - loads the list of ducks
        public void OnGet()
        {
            LoadDuckList(); // Populate the dropdown list with available ducks
        }

        // Handles HTTP POST requests (when user selects a duck)
        // Loads the duck list and retrieves the selected duck's details
        public IActionResult OnPost()
        {
            LoadDuckList(); // Refresh the dropdown list
            
            // If a valid duck ID is selected, fetch its details
            if (SelectedDuckId != 0)
            {
                SelectedDuck = GetDuckById(SelectedDuckId);
            }
            return Page(); // Return the updated page
        }

        // Helper method that loads the list of ducks from the SQLite database
        // This method populates the dropdown menu with available ducks
        private void LoadDuckList()
        {
            DuckList = new List<SelectListItem>(); // Initialize the list
            
            using (var connection = new SqliteConnection("Data Source=RubberDucks.db"))
            {
                connection.Open(); // Open the database connection
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name FROM Ducks"; // SQL query to fetch ducks
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) // Read each duck from the database
                    {
                        DuckList.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(), // Duck ID as the value
                            Text = reader.GetString(1)             // Duck name as the display text
                        });
                    }
                }
            }
        }

        // Helper method that retrieves a specific duck by its ID from the database
        // Returns all details of the selected duck
        private Duck? GetDuckById(int id)
        {
            using (var connection = new SqliteConnection("Data Source=RubberDucks.db"))
            {
                connection.Open(); // Open the database connection
                var command = connection.CreateCommand();
                
                // SQL query to fetch details of the selected duck using a parameterized query
                command.CommandText = "SELECT * FROM Ducks WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id); // Prevents SQL injection
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // If a matching duck is found, create an object
                    {
                        return new Duck
                        {
                            Id = reader.GetInt32(0), // Duck ID
                            Name = reader.GetString(1), // Duck name
                            Description = reader.GetString(2), // Duck description
                            Price = reader.GetDecimal(3), // Duck price
                            ImageFileName = reader.GetString(4) // Image file name
                        };
                    }
                }
            }
            return null; // Return null if no duck is found
        }
    }

    // Simple model class representing a rubber duck product
    public class Duck
    {
        public required int Id { get; set; } // Unique identifier for the duck
        public required string Name { get; set; } // Name of the duck
        public required string Description { get; set; } // Description of the duck
        public required decimal Price { get; set; } // Price of the duck
        public required string ImageFileName { get; set; } // Image filename for display
    }
}
