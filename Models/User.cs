using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Homera.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;


        // Navigation properties

        // Client -> Tasks
        public virtual ICollection<TaskItem>? CreatedTasks { get; set; }

        // Housekeeper -> Assigned tasks
        public virtual ICollection<TaskItem>? AssignedTasks { get; set; }

        // Client -> Locations
        public virtual ICollection<Location>? Locations { get; set; }
    }
}
