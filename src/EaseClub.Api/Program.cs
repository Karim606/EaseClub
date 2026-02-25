using DotNetEnv;
using EaseClub.Application;
using Serilog;
using BetterStack.Logs.Serilog;
using Microsoft.OpenApi;
using Serilog.Core;
using System.Text.Json;
using System.Text;
using EaseClub.Infrastructure.Data;
namespace EaseClub.Api
{
    public class Program
    {
        
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var envName = builder.Environment.EnvironmentName.ToLower();

            // This finds the folder where the .sln usually sits
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.Parent?.FullName;
            var envPath = Path.Combine(projectRoot ?? "", ".env." + envName.ToLower());

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }

            builder.Configuration.AddEnvironmentVariables();


            builder.Services.AddPresentation(builder.Configuration)
                            .AddApplication()
                            .AddInfrastructure(builder.Configuration,builder.Environment);

            builder.Host.UseSerilog((context, loggerConfig) => {
                loggerConfig.ReadFrom.Configuration(context.Configuration);

                var sourceToken = builder.Configuration["BETTERSTACK_SOURCE_TOKEN"];
                var ingestHost = builder.Configuration["BETTERSTACK_INGEST_URL"];
                loggerConfig.WriteTo.BetterStack(sourceToken: sourceToken, betterStackEndpoint: ingestHost);
                
                
            });



            var app = builder.Build();
          
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()|| app.Environment.IsProduction())
            {
                app.UseSwagger(); 
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EaseClub API V1");
                    c.EnableDeepLinking();
                    c.DisplayRequestDuration();
                    c.EnableFilter();
                });
            }
          
            if(!app.Environment.IsEnvironment("testing"))
            await app.Init();

            app.UseCoreMiddlewares(builder.Configuration);

            app.MapControllers();

            app.Run();
        }
    }
}
