using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ShareBook.Api.Configuration;
using ShareBook.Api.Middleware;
using ShareBook.Api.Services;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.Muambator;
using ShareBook.Service.Notification;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using System;

namespace ShareBook.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterHealthChecks(services, Configuration.GetConnectionString("DefaultConnection"));

            services
                .RegisterRepositoryServices();

            // Auto Mapper Configurations
            services
                .AddAutoMapper(typeof(Startup));

            services
                .Configure<ImageSettings>(options => Configuration.GetSection("ImageSettings").Bind(options))
                .Configure<EmailSettings>(options => Configuration.GetSection("EmailSettings").Bind(options))
                .Configure<ServerSettings>(options => Configuration.GetSection("ServerSettings").Bind(options))
                .Configure<NotificationSettings>(options => Configuration.GetSection("NotificationSettings").Bind(options));

            services
                .AddHttpContextAccessor();

            JWTConfig.RegisterJWT(services, Configuration);

            services
                .ConfigureSwaggerData();

            services
                .AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services
                .AddDbContext<ApplicationDbContext>(options =>
                        options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                                mySqlOptions =>
                                {
                                    mySqlOptions.ServerVersion(new Version(8, 5), ServerType.MySql);
                                    mySqlOptions.EnableRetryOnFailure(2);
                                    mySqlOptions.CharSetBehavior(CharSetBehavior.AppendToAllColumns);
                                }
                        )
                    );

            //RollbarConfigurator.Configure(Configuration["Rollbar:Environment"], Configuration["Rollbar:AccessToken"]);
            services
                .ConfigureRollbar(Configuration["Rollbar:Environment"], Configuration["Rollbar:AccessToken"]);

            MuambatorConfigurator.Configure(Configuration.GetSection("Muambator:Token").Value, Configuration.GetSection("Muambator:IsActive").Value);

            services
                .AddControllers()
                .AddFluentValidation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction() || env.IsStaging())
                app.UseRollbarMiddleware();

            app.UseDeveloperExceptionPage();
            app.UseExceptionHandlerMiddleware();

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger", "SHAREBOOK API V2");
                });

            app.UseRouting();

            app.UseCors(x =>
            {
                x.AllowAnyOrigin();
                x.AllowAnyMethod();
                x.AllowAnyHeader();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Book}/{action=Index}/{id?}");

                endpoints.MapFallbackToController("Index", "ClientSpa");

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    AllowCachingResponses = false,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scopeServiceProvider = serviceScope.ServiceProvider;
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                if (env.IsDevelopment() || env.IsStaging())
                {
                    var sharebookSeeder = new ShareBookSeeder(context);
                    sharebookSeeder.Seed();
                }
            }
        }

        private void RegisterHealthChecks(IServiceCollection services, string connectionString)
        {
            services.AddHealthChecks()
                .AddMySql(connectionString);
        }

    }
}