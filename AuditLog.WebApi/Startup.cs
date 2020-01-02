using System;
using System.Diagnostics.CodeAnalysis;
using AuditLog.Abstractions;
using AuditLog.DAL;
using AuditLog.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minor.Miffy.RabbitMQBus;

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
                                   "server=localhost;uid=root;pwd=root;database=AuditLogDB";

            services.AddDbContext<AuditLogContext>(optionsBuilder => optionsBuilder
                .UseMySql(connectionString));
            services.AddTransient<IAuditLogRepository<LogEntry, long>, AuditLogRepository>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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