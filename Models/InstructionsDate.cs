using System.ComponentModel.DataAnnotations;

namespace dotInstrukcijeBackend.Models
{
    public class InstructionsDate
    {
        [Key]
        public int id { get; set; }
        public int studentId { get; set; }
        public int professorId { get; set; }
        public DateTime dateTime { get; set; }
        public String status { get; set; }

    }
}
