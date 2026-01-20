using FlliBrutti.Backend.Application.Crittography;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.ICrittography;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Services;
using FlliBrutti.Backend.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Map(
        keyPropertyName: "SourceContext",
        configure: (sourceContext, wt) => wt.File(
            path: $"Logs/{DateTime.Now:yyyy-MM-dd}/{sourceContext}.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ),
        sinkMapCountLimit: 50)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert JWT token with Bearer prefix. Example: 'Bearer {token}'"
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

// JWT Authentication Configuration
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer not configured in appsettings");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience not configured in appsettings");

// Cookie name constant
const string ACCESS_TOKEN_COOKIE = "access_token";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // In produzione, impostare a true
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero // Elimina il margine di 5 minuti di default
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Prima controlla se c'è un cookie
            if (context.Request.Cookies.ContainsKey(ACCESS_TOKEN_COOKIE))
            {
                context.Token = context.Request.Cookies[ACCESS_TOKEN_COOKIE];
            }
            // Fallback: controlla l'header Authorization (utile per Swagger/testing)
            else if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "You are not authorized",
                message = context.ErrorDescription ?? "Token is missing or invalid"
            });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "You do not have permission to access this resource"
            });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<FlliBruttiContext>(opt =>
{
    opt.UseMySql(
        builder.Configuration.GetConnectionString("FlliBruttiDatabase"),
        new MySqlServerVersion(new Version(8, 0, 33))
    );
});

// Context
builder.Services.AddScoped<IFlliBruttiContext, FlliBruttiContext>();

// Application Services
builder.Services.AddScoped<IFirmaService, FirmaService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPreventivoNCCService, PreventivoNCCService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Crittography must be singleton
builder.Services.AddSingleton<IPasswordHash, PasswordHash>(sp =>
{
    var secret = builder.Configuration["Security:Secret"];
    if (string.IsNullOrEmpty(secret))
    {
        throw new InvalidOperationException("The Secret was not found in appsettings");
    }
    return new PasswordHash(secret);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // ⚠️ IMPORTANTE: deve essere PRIMA di UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();