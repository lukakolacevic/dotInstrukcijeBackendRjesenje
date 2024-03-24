using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Models;
using Microsoft.AspNetCore.Authorization;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AppDatabaseContext context;

        public SubjectController(AppDatabaseContext context)
        {
            this.context = context;
        }
        
        [Authorize]
        [HttpPost("subject")]
        
        public async Task<IActionResult> CreateSubject([FromBody] SubjectRegistrationModel request)
        {
            //jedino profesorima treba dopustiti da stvaraju nove predmete
            if (!User.IsInRole("Professor"))
            {
                return Unauthorized("Unauthorized to create new subject"); 
            }
            // Provjera postoji li već predmet s istim naslovom ili kraticom
            if (context.Subjects.Any(s => s.title == request.title || s.url == request.url))
            {
                return BadRequest(new { success = false, message = "Subject with given title or URL already exists." });
            }

            var subject = new Subject
            {
                title = request.title,
                url = request.url,
                description = request.description
            };

            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            return Ok(new { success = true, message = "Subject created successfully." });
        }

        

        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await context.Subjects
                                         .Select(subject => new
                                         {
                                             subject.id,
                                             subject.title,
                                             subject.url,
                                             subject.description
                                         })
                                         .ToListAsync();

            return Ok(new
            {
                success = true,
                subjects = subjects
            });
        }

    }

}
