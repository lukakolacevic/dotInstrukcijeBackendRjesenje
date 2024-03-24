using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Professor
    {
        [Key]
        public int Id { get; set; } 
        public String email { get; set; }
        public String name { get; set; }
        public String surname { get; set; }
        public String password { get; set; }
        public String profilePictureUrl { get; set; }
        public int instructionsCount { get; set; }
        public String[] subjects { get; set; }

        
    }
}