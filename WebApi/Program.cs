using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.InfProfile;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddConnections();
builder.Services.AddAutoMapper(typeof(InfrastructureProfile));
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IBaseRepository<Student, int>, StudentRepository>();
builder.Services.AddDbContext<DataContext>(t => 
    t.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "WebApi")); 
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}