using FirstContact.Api.Endpoints;
using FirstContact.Application;
using FirstContact.Infrastructure;
using FirstContact.Infrastructure.Knowledge;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Let the Angular dev server call the API while developing.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins("http://localhost:4300")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Create the SQLite file and load the handbook on first start, so there is
// nothing to set up by hand after cloning.
using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FirstContactDbContext>().Database.EnsureCreated();

    await scope.ServiceProvider.GetRequiredService<HandbookSeeder>()
        .SeedAsync(Path.Combine(app.Environment.ContentRootPath, "Handbook"), CancellationToken.None);
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapRequests();
app.MapKnowledge();
app.MapDashboard();
app.MapFeatures();

app.Run();

/// <summary>
/// Minimal APIs generate an internal Program class. Making it public lets the test
/// project start the whole application in memory.
/// </summary>
public partial class Program;
