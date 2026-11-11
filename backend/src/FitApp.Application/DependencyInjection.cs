using System.Reflection;
using FitApp.Application.Common.Behaviors;
using FitApp.Application.Features.Sync;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FitApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<Engines.InsightService>();
        services.AddScoped<Features.Gamification.BadgeService>();

        RegisterSyncHandlers(services, assembly);

        return services;
    }

    private static void RegisterSyncHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(ISyncAggregateHandler).IsAssignableFrom(t));

        foreach (var type in handlerTypes)
            services.AddScoped(typeof(ISyncAggregateHandler), type);
    }
}
