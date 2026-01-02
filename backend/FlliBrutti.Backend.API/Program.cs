using Serilog;
using FlliBrutti.Backend.Infrastructure.Database;
using FlliBrutti.Backend.Application.IContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log\\", rollingInterval: RollingInterval.Day,
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = false;
    });

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

app.UsePathBase("/api/v1/");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
