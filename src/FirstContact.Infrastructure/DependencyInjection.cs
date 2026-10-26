using FirstContact.Application.Abstractions;
using FirstContact.Application.Knowledge;
using FirstContact.Application.Requests;
using FirstContact.Infrastructure.Ai;
using FirstContact.Infrastructure.Dashboard;
using FirstContact.Infrastructure.Features;
using FirstContact.Infrastructure.Knowledge;
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
        KnowledgeOptions knowledge = Read<KnowledgeOptions>(configuration, KnowledgeOptions.SectionName);
        KnowledgeSearchOptions searching =
            Read<KnowledgeSearchOptions>(configuration, KnowledgeSearchOptions.SectionName);
        FeatureMatchOptions matching =
            Read<FeatureMatchOptions>(configuration, FeatureMatchOptions.SectionName);

        services.AddSingleton(azure);
        services.AddSingleton(triage);
        services.AddSingleton(knowledge);
        services.AddSingleton(searching);
        services.AddSingleton(matching);

        services.AddDbContext<FirstContactDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Database") ?? "Data Source=firstcontact.db"));

        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IClassifier, AzureClassifier>();

        services.AddScoped<IKnowledgeGapRepository, KnowledgeGapRepository>();
        services.AddSingleton<IEmbedder, AzureEmbedder>();
        services.AddScoped<IKnowledgeSearch, VectorSearch>();
        services.AddScoped<IAnswerWriter, AzureAnswerWriter>();
        services.AddScoped<HandbookSeeder>();
        services.AddScoped<IDashboardQueries, DashboardQueries>();
        services.AddScoped<IFeatureDemandRepository, FeatureDemandRepository>();
        services.AddScoped<IFeatureMatcher, EmbeddingFeatureMatcher>();

        return services;
    }

    private static TOptions Read<TOptions>(IConfiguration configuration, string section)
        where TOptions : new() =>
        configuration.GetSection(section).Get<TOptions>() ?? new TOptions();
}
