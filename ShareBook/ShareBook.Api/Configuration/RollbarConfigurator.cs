using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rollbar;
using Rollbar.NetCore.AspNet;

namespace ShareBook.Api.Services
{
    public static class RollbarConfigurator
    {
        public static IServiceCollection ConfigureRollbar(this IServiceCollection services, string environment, string accessToken)
        {
            if (string.IsNullOrEmpty(environment) || environment == "Dev")
                return services;

            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace;
            });

            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(accessToken) { Environment = environment });
            RollbarLocator.RollbarInstance.Info($"Rollbar is configured properly in {environment} environment.");

            return services;
        }
    }
}
