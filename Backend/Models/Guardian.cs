using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Guardian
    {
        [Key]
        public int GuardianID { get; set; }
        [Required]
        public Name FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Job { get; set; }
        public string? TypeGuardian { get; set; }
        public string? Description { get; set; }
        public int UserID { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}