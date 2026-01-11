using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Services;
using FlliBrutti.Backend.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);

// Serilog
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

// 🔴 OBBLIGATORIO
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Db
builder.Services.AddDbContext<FlliBruttiContext>(opt =>
{
    opt.UseMySql(
        builder.Configuration.GetConnectionString("FlliBruttiDatabase"),
        new MySqlServerVersion(new Version(8, 0, 33))
    );
});

builder.Services.AddScoped<IFlliBruttiContext, FlliBruttiContext>();
builder.Services.AddScoped<IFirmaService, FirmaService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPreventivoNCCService, PreventivoNCCService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UsePathBase("/v1/");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
