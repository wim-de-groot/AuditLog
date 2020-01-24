using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuditLog.Abstractions;
using AuditLog.DAL;
using AuditLog.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuditLog.WebApi
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                                   throw new InvalidEnvironmentException(
                                       "Environment variable [CONNECTION_STRING] was not provided.");

            services.AddDbContext<AuditLogContext>(optionsBuilder => optionsBuilder
                .UseMySql(connectionString));
            services.AddTransient<IAuditLogRepository<LogEntry, long>, AuditLogRepository>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
                
            var createdAndSeeded = false;
            const int waitTime = 1000;
            while (!createdAndSeeded)
            {
                try
                {
                    serviceScope.ServiceProvider.GetService<AuditLogContext>().Database.EnsureCreated();
                    createdAndSeeded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(waitTime);
                }
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}