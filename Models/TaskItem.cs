using System;
using System.ComponentModel.DataAnnotations;
using Homera.Models.Enums;

namespace Homera.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal Budget { get; set; }

        public DateTime Deadline { get; set; }

        public TaskCategory Category { get; set; }

        public TaskItemStatus Status { get; set; }

        // Date when housekeeper sends it for review
        public DateTime? ReviewDate { get; set; }

        // Image proof path
        public string ImagePath { get; set; }

        // CLIENT (Creator)
        public int ClientId { get; set; }
        public User Client { get; set; }

        // HOUSEKEEPER (Assigned)
        public int? HousekeeperId { get; set; }
        public User Housekeeper { get; set; }

        // LOCATION
        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
