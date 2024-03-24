namespace dotInstrukcijeBackend.ViewModels
{
    public class ProfessorRegistrationModel
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string profilePictureUrl { get; set; }

        public string[] subjects { get; set; }
    }
}
