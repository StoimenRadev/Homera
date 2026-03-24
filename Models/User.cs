using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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


        // Navigation properties

        // Client -> Tasks
        public virtual ICollection<TaskItem>? CreatedTasks { get; set; }

        // Housekeeper -> Assigned tasks
        public virtual ICollection<TaskItem>? AssignedTasks { get; set; }

        // Client -> Locations
        public virtual ICollection<Location>? Locations { get; set; }
    }
}
