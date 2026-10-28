using FirstContact.Application.Dashboard;
using FirstContact.Application.Features;
using FirstContact.Application.Knowledge;
using FirstContact.Application.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace FirstContact.Application;

/// <summary>Registers the use cases. The layer registers itself, so the API need not know them.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<HandleIncomingMessage>();
        services.AddScoped<GetReviewQueue>();
        services.AddScoped<ResolveReview>();
        services.AddScoped<AnswerFromHandbook>();
        services.AddScoped<WriteArticle>();
        services.AddScoped<GetDashboard>();
        services.AddScoped<GroupFeatureWish>();

        return services;
    }
}
