using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Student
    {
        [Key]
        public int id { get; set; }
        public String email { get; set; }
        public String name  { get; set; }
        public String surname { get; set; }
        public String password { get; set; }
        public String profilePictureUrl { get; set; }

        
    }
}

