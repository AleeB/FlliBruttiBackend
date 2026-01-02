using Serilog;
using FlliBrutti.Backend.Infrastructure.Database;
using FlliBrutti.Backend.Application.IContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log\\", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .MinimumLevel.Information()
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UsePathBase("/api/v1");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
