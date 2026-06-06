using EaseClub.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using EaseClub.Application.Features.Enrollments.Services;
using EaseClub.Application.Features.Events.Services;
using System.Reflection;
namespace EaseClub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            });

            services.AddScoped<EnrollmentManager>();
            services.AddScoped<EventRegistrationService>();

            return services;
        }
    }
}
