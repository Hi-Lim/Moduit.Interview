using Microsoft.Extensions.DependencyInjection;
using Moduit.Interview.Common.Commands;
using System;

namespace Moduit.Interview.Api.AspNetCore.Extensions
{
    public static class ModuitExtension
    {
        public static IServiceCollection AddModuitExtension(this IServiceCollection services, ModuitConfiguration moduitConfiguration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (moduitConfiguration != null)
            {
                services.AddSingleton(moduitConfiguration);
            }
            return services;
        }
    }
}
