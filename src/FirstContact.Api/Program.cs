var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Confirms the API is running. Used by the tests and by Docker later.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

/// <summary>
/// Minimal APIs generate an internal Program class. Making it public lets the
/// test project start the whole application in memory.
/// </summary>
public partial class Program;
