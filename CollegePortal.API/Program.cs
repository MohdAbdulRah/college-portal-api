using System.Text;
using CollegePortal.API.Data;
using CollegePortal.API.Middleware;
using CollegePortal.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════ Database ══════════════════
builder.Services.AddDbContext<CollegePortalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ══════════════════ Services DI ══════════════════
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ══════════════════ Controllers ══════════════════
builder.Services.AddControllers();

// ══════════════════ JWT Authentication ══════════════════
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Read SecretKey: try config section first, then direct env var (Render Docker fallback)
var secretKeyStr = jwtSettings["SecretKey"]
    ?? Environment.GetEnvironmentVariable("Jwt__SecretKey")
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException(
        "JWT SecretKey is not configured. Set the 'Jwt__SecretKey' environment variable on Render.");

var jwtIssuer = jwtSettings["Issuer"]
    ?? Environment.GetEnvironmentVariable("Jwt__Issuer") ?? "CollegePortal.API";

var jwtAudience = jwtSettings["Audience"]
    ?? Environment.GetEnvironmentVariable("Jwt__Audience") ?? "CollegePortal.Client";

var secretKey = Encoding.UTF8.GetBytes(secretKeyStr);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ══════════════════ CORS ══════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ══════════════════ Swagger / OpenAPI ══════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Landmine Soft College Portal API",
        Version = "v1",
        Description = "Complete college management portal API with JWT authentication, role-based access control, and full CRUD operations.",
        Contact = new OpenApiContact
        {
            Name = "Team Landmine Soft",
            Email = "support@landminesoft.edu"
        }
    });

    // JWT Bearer token support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ══════════════════ Middleware Pipeline ══════════════════
app.UseMiddleware<ExceptionMiddleware>();

// Swagger always enabled for demo purposes
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "College Portal API v1");
    options.RoutePrefix = "swagger";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    application = "Landmine Soft College Portal API",
    version = "1.0",
    status = "Running",
    documentation = "/swagger",
    timestamp = DateTime.UtcNow
}));

// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CollegePortalDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
