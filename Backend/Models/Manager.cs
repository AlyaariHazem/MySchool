using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Manager
    {
        [Required]
        public int ManagerID { get; set; }
        [Required]
        public Name FullName { get; set; }
        public DateTime? DOB { get; set; } = DateTime.Now;
        public string? ImageURL { get; set; }
        public string UserID { get; set; }
        [JsonIgnore]
        public School School { get; set; }
        [Required]
        public int SchoolID { get; set; }
        public int? TenantID { get; set; }
        public Tenant Tenant { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        // public virtual ICollection<Teacher> Teachers { get; set; }

    }
}