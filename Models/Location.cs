using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Homera.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        // Client owner
        public int ClientId { get; set; }
        public User Client { get; set; }

        // Tasks in this location
        public ICollection<TaskItem> Tasks { get; set; }
    }
}
