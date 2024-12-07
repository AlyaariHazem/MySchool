using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string? Type{ get; set; }
        public string UserID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<AccountStudentGuardian> AccountStudentGuardians { get; set; }
         
    }
}