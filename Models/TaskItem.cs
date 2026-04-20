using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using Homera.Models.Enums;

namespace Homera.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public decimal Budget { get; set; }

        public DateTime Deadline { get; set; }

        public TaskCategory Category { get; set; }

        public TaskItemStatus Status { get; set; }

        [Display(Name = "Дата на изпращане")]
        public DateTime? ReviewDate { get; set; }

        [Display(Name = "Снимка доказателство")]
        public string? ImagePath { get; set; }
 
        [NotMapped]
        [Display(Name = "Snimka (Proof)")]
        public IFormFile? FileProof { get; set; }

        // CLIENT (Creator)
        public int ClientId { get; set; }
        public virtual User? Client { get; set; }

        [Display(Name = "Помощник")]
        public int? HousekeeperId { get; set; }
        public virtual User? Housekeeper { get; set; }

        // LOCATION
        public int LocationId { get; set; }
        public virtual Location? Location { get; set; }
    }
}
