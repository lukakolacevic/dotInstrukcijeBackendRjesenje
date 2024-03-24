using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class Subject
    {
        [Key]
        public int id { get; set; }
        public String title { get; set; }
        public String url { get; set; }
        public String description { get; set; }

       
    }
}
