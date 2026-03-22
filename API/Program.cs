using API.Middleware;
using Application.Core;
using Application.Events.Validators;
using Application.Tickets.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Event Ticketing System API",
        Version = "v1",
        Description = "A comprehensive API for managing events and ticket sales",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Event Ticketing System",
            Email = "support@eventticketingsystem.com"
        }
    });
});
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MappingProfiles).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.LicenseKey = builder.Configuration["Licences:MediatR"];
});
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// Register all validators from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PurchaseTicketValidator>();

builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for easier access
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Ticketing System API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000", "https://localhost:3000"));
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

// Create a new scope and context for seeding to ensure fresh connection
try
{
    using var seedScope = app.Services.CreateScope();
    var seedServices = seedScope.ServiceProvider;
    var seedContext = seedServices.GetRequiredService<AppDbContext>();
    
    await EventTicketingSeeder.SeedData(seedContext);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during seeding");
}


app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
