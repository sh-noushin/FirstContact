using FirstContact.Application.Abstractions;
using FirstContact.Application.Requests;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FirstContact.Infrastructure;

/// <summary>Registers everything that talks to the outside world: SQLite and Azure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AzureAiOptions azure = Read<AzureAiOptions>(configuration, AzureAiOptions.SectionName);
        TriageOptions triage = Read<TriageOptions>(configuration, TriageOptions.SectionName);

        services.AddSingleton(azure);
        services.AddSingleton(triage);

        services.AddDbContext<FirstContactDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Database") ?? "Data Source=firstcontact.db"));

        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IClassifier, AzureClassifier>();

        return services;
    }

    private static TOptions Read<TOptions>(IConfiguration configuration, string section)
        where TOptions : new() =>
        configuration.GetSection(section).Get<TOptions>() ?? new TOptions();
}
