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
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TypeGuardian { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public virtual ICollection<Accounts> Accounts { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}