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

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly AppDatabaseContext context;

        private readonly IConfiguration configuration;

        public ProfessorController(AppDatabaseContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("register/professor")]
        public async Task<IActionResult> Register([FromBody] ProfessorRegistrationModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if (await context.Professors.AnyAsync(s => s.email == model.email))
            {
                return BadRequest(new { success = false, message = "Email is already in use." });
            }

            var professor = new Professor
            {
                email = model.email,
                name = model.name,
                surname = model.surname,
                password = PasswordHasher.HashPassword(model.password),
                subjects = model.subjects,
                profilePictureUrl = model.profilePictureUrl,
                instructionsCount = 0     //postavi broj instrukcija na nulu kad se stvori novi profeso
            };

            context.Professors.Add(professor);
            await context.SaveChangesAsync();

            var response = new
            {
                success = true,
                message = "Professor registered successfully!"
            };
            return Ok(response);
        }

        private string GenerateJwtToken(Professor professor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, professor.email),
                    new Claim("id", professor.Id.ToString()),
                    new Claim(ClaimTypes.Name, professor.name + " " + professor.surname),
                    new Claim(ClaimTypes.Role, "Professor")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("login/professor")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var professor = await context.Professors
                .FirstOrDefaultAsync(p => p.email == model.email);

            if (professor == null)
            {
                return BadRequest(new { success = false, message = "Professor not found." });
            }

            if (!PasswordHasher.VerifyPassword(professor.password, model.password))
            {
                return BadRequest(new { success = false, message = "Invalid password." });
            }

            // Generiraj profesorov JWT token 
            var token = GenerateJwtToken(professor);

            return Ok(new
            {
                success = true,
                professor = professor, 
                token,
                message = "Login successful."
            });
        }

        [Authorize]
        [HttpGet("professor/{email}")]

        public async Task<IActionResult> GetProfessorByEmail(string email)
        {
            var professor = await context.Professors
                                        .Where(p => p.email == email)
                                        .FirstOrDefaultAsync();

            if (professor == null)
            {
                return NotFound(new { success = false, message = "Professor not found." });
            }

            return Ok(new { success = true, professor = professor, message = "Professor found successfully!" });
        }

        [Authorize]
        [HttpGet("professors")]

        public async Task<IActionResult> GetAllStudents()
        {
            var listOfProfessors = await context.Professors.ToListAsync();

            return Ok(new { success = true, listOfProfessors= listOfProfessors, message = "All professors returned successfully!" });
        }
    }
}
