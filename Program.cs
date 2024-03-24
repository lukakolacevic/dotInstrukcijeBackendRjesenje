using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.PasswordHashingUtilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddCors(options => 
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:5173") 
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials(); 
            });
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "dotInstrukcijeBackend", Version = "v1" });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", 
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] {}}
    });
        });


        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        SetUpDB(builder);
        SetUpJWT(builder);

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();



        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //InitializeDatabase(app.Services);
        TestDatabaseConnection(app.Services);

        app.UseCors("CorsPolicy");

        //app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }

    private static void SetUpDB(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDatabaseContext>(options =>
                        options.UseSqlite(builder.Configuration.GetConnectionString("AppContextExampleConnection")));
    }

    private static void SetUpJWT(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

        builder.Services.AddAuthorization();
    }

    private async static void TestDatabaseConnection(IServiceProvider services)
    {
        await Task.Delay(3000);

        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
            try
            {
                dbContext.Database.OpenConnection();
                dbContext.Database.CloseConnection();
                Console.WriteLine("Database connection successful.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
            }
        }
    }
    /*
    public static async Task InitializeDatabase(IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();

            // Kreiranje novog studenta
            var student = new Student
            {
                email = "tompakolacevic@gmail.com",
                name = "Tomica",
                surname = "Kolacevic",
                password = PasswordHasher.HashPassword("golfgti"),
                profilePictureUrl = "example"
            };


            // Dodajte studenta u DbContext
            dbContext.Students.Add(student);

            // Spremite promjene u bazu
            await dbContext.SaveChangesAsync();

        }
    }
    */
}