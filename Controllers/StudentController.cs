using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.PasswordHashingUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using NServiceBus.Testing;
using Microsoft.AspNetCore.Hosting;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AppDatabaseContext context;

        private readonly IConfiguration configuration;

        private readonly IWebHostEnvironment hostingEnvironment;

        public StudentController(AppDatabaseContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            this.context = context;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }


        [HttpPost("register/student")]
        public async Task<IActionResult> Register([FromBody] StudentRegistrationModel model)
        {
            // Provjeri ispravnost poslanih podataka
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            // Provjeri postoji li već student s istim emailom
            if (await context.Students.AnyAsync(s => s.email == model.email))
            {
                return BadRequest(new { success = false, message = "Email is already in use." });
            }

            
            string profilePictureUrl = await SaveProfilePicture(model.profilePicture);

            
            var student = new Student
            {
                email = model.email,
                name = model.name,
                surname = model.surname,
                password = PasswordHasher.HashPassword(model.password),
                profilePictureUrl = profilePictureUrl
            };

            context.Students.Add(student);
            await context.SaveChangesAsync();

            var response = new
            {
                success = true,
                message = "Student registered successfully!"
            };
            return Ok(response);
        }
        //metoda za pretvaranje slike u URL


        private async Task<string> SaveProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return "default-profile-pic-url"; 
            }

            // Putanja do foldera gdje će se slike spremiti
            var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "profile-pictures");

            // Ako direktorij ne postoji, stvorite ga
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            
            var fileName = Path.GetFileNameWithoutExtension(profilePicture.FileName);
            var extension = Path.GetExtension(profilePicture.FileName);
            fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            // Vraćate URL-a do slike za spremanje u bazu
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            return $"{baseUrl}/profile-pictures/{fileName}";
        }



        [HttpPost("login/student")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var student = await context.Students
                .FirstOrDefaultAsync(s => s.email == model.email);

            if (student == null)
            {
                return BadRequest(new { success = false, message = "User not found." });
            }

            if (!PasswordHasher.VerifyPassword(student.password, model.password))
            {
                return BadRequest(new { success = false, message = "Invalid password." });
            }

            // Generiraj studentov JWT token 
            var token = GenerateJwtToken(student);

            return Ok(new
            {
                success = true,
                student = new { student.email, student.name, student.surname, student.password, student.profilePictureUrl}, // Prilagodite prema potrebama
                token,
                message = "Login successful."
            });
        }

        private string GenerateJwtToken(Student student)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Email, student.email),
                new Claim("id", student.id.ToString())
                
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpGet("student/{email}")]

        public async Task<IActionResult> GetStudentByEmail(string email)
        {
            var student = await context.Students
                                        .Where(s => s.email == email)
                                        .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            return Ok(new { success = true, student = student, message = "Student found successfully!" });
        }

        [Authorize]
        [HttpGet("students")]

        public async Task<IActionResult> GetAllStudents()
        {
            var listOfStudents = await context.Students.ToListAsync();

            return Ok(new {success = true, listOfStudents = listOfStudents, message = "All students returned successfully!" });
        }
    }

        
        
        

    
}
