using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homera.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Street { get; set; } = null!;

        [Required]
        [Display(Name = "No.")]
        public string HouseNumber { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        public string? Neighbourhood { get; set; }

        public string? PostalCode { get; set; }

        [Required]
        public string Country { get; set; } = null!;

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        // Client owner
        public int ClientId { get; set; }
        public virtual User? Client { get; set; }

        // Tasks in this location
        public virtual ICollection<TaskItem>? Tasks { get; set; }

        [NotMapped]
        public string DisplayName => $"{Name} - {Street} {HouseNumber}, {City}";
    }
}
