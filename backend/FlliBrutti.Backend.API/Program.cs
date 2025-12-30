using Serilog;
using FlliBrutti.Backend.Infrastructure.Database;
using FlliBrutti.Backend.Application.IContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("C:\\Unicam\\Unicam.Paradigmi\\Unicam.Libreria.Web\\bin\\Debug\\net8.0\\log.txt", rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7
    )
    // set default minimum level
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FlliBruttiContext>(opt =>
{
    opt.UseMySql(
        builder.Configuration.GetConnectionString("FlliBruttiDatabase"),
        new MySqlServerVersion(new Version(8, 0, 33))
    );
});

builder.Services.AddScoped<IFlliBruttiContext, FlliBruttiContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();
