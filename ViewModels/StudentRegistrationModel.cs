namespace dotInstrukcijeBackend.ViewModels
{
    public class StudentRegistrationModel
    {
        public string name { get;  set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public IFormFile profilePicture { get; set; } //stavlja se slika koja se onda pretvara u URL
    }
}
