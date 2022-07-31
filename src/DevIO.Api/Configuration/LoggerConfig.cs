using DevIO.Api.Extensions;
using Elmah.Io.Extensions.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var apiKey = configuration["Elmah:ApiKey"]; 
            var logId = configuration["Elmah:LogId"];
            var heartbeatId = configuration["Elmah:HeartbeatId"];

            services.AddElmahIo(o =>
            {
                o.ApiKey = apiKey;
                o.LogId = new Guid(logId);
            });

            services.AddHealthChecks()
                .AddElmahIoPublisher(options =>
                {
                    options.ApiKey = apiKey;
                    options.LogId = new Guid(logId);
                    options.HeartbeatId = heartbeatId;

                })
                .AddCheck("Produtos", new MySqlHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddMySql(configuration.GetConnectionString("DefaultConnection"), name: "Database");
            
            services.AddHealthChecksUI()
                .AddMySqlStorage(configuration.GetConnectionString("DefaultConnection"));

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            app.UseHealthChecks("/api/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }
    }
}