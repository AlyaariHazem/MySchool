using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class User
    {
        public int UserID { get; set; }
        [Required]
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [Required]
        public string UserType { get; set; }

        // Navigation properties for specific user roles
        [JsonIgnore]
        public Guardian Guardian { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }
        [JsonIgnore]
        public Student Student { get; set; }
        [JsonIgnore]
        public Manager Manager { get; set; }
    }
}