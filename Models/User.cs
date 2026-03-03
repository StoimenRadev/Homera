using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Homera.Models.Enums;

namespace Homera.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public UserRole Role { get; set; }

        // Navigation properties

        // Client -> Tasks
        public ICollection<TaskItem> CreatedTasks { get; set; }

        // Housekeeper -> Assigned tasks
        public ICollection<TaskItem> AssignedTasks { get; set; }

        // Client -> Locations
        public ICollection<Location> Locations { get; set; }
    }
}
