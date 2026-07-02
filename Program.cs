using Majstor_bob.Data;
using Majstor_bob.Helper;
using Majstor_bob.Interfaces;
using Majstor_bob.Models;
using Majstor_bob.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IKorisniciRepository, KorisniciRepository>();
builder.Services.AddScoped<IKlijentiRepository, KlijentiRepository>();
builder.Services.AddScoped<IFirmeRepository, FirmeRepository>();
builder.Services.AddScoped<IMajstoriRepository, MajstoriRepository>();

builder.Services.AddScoped<IOcenaRepository, OcenaRepository>();
builder.Services.AddScoped<IZahteviRepository, ZahtevRepository>();
builder.Services.AddScoped<IZakazivanjeRepository, ZakazivanjeRepository>();
builder.Services.AddScoped<IObavestenjaRepository, ObavestenjeRepository>();
builder.Services.AddScoped<IPrijaveRepository, PrijaveRepository>();
builder.Services.AddScoped<IRazgovorRepository, RazgovorRepository>();
builder.Services.AddScoped<IPorukeRepository, PorukeRepository>();

builder.Services.AddScoped<IKategorijeRepository, KategorijaRepository>();
builder.Services.AddScoped<IGradoviRepository, GradoviRepository>();
builder.Services.AddScoped<IPripadaRepository, PripadaRepository>();
builder.Services.AddScoped<IGradoviradaRepository, GradRadaRepository>();
builder.Services.AddScoped<TokenProvider>();

builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("StripeSettings")
);


//Dto data tranfer object vljd sta frontu saljem
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//OVO SAM JA DODALA DA PROBAM FRONT
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});




//JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("KlijentOnly", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Klijent"));
    options.AddPolicy("MajstorOnly", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Majstor"));
    options.AddPolicy("FirmaOnly", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Firma"));
});
//[Authorize(Role = "Admin")]

//---TEST AAUTHENTICATION IN SWAGER--

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//

// Read connection string
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

var key = builder.Configuration["Encryption:Key"];
var IV = builder.Configuration["Encryption:IV"];
// Register it
builder.Services.AddSingleton(connectionString);

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//I OVO SAM DODALA
app.UseCors("AllowFrontend");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();
/*
 *host: mysql-45f7888-majstorbob.j.aivencloud.com
 * port:16801
 * database:majstorbob
 * user:dimitrije
 * sifra:AVNS_V6QZb02iiyzZyPpZ0Io
 * 
 */