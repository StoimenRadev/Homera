using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Homera.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        // Client owner
        public int ClientId { get; set; }
        public virtual User? Client { get; set; }

        // Tasks in this location
        public virtual ICollection<TaskItem>? Tasks { get; set; }
    }
}
