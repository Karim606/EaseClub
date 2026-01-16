using DotNetEnv;
using EaseClub.Application;
using Serilog;
using BetterStack.Logs.Serilog;
using Microsoft.OpenApi;
using Serilog.Core;
using System.Text.Json;
using System.Text;
namespace EaseClub.Api
{
    public class Program
    {
        
        public  static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var EnvName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production";
            Env.Load("../../.env." + EnvName.ToLower());
            // Add services to the container.

            builder.Configuration.AddEnvironmentVariables();


            builder.Services.AddPresentation(builder.Configuration)
                            .AddApplication()
                            .AddInfrastructure(builder.Configuration);

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
            


            app.UseCoreMiddlewares(builder.Configuration);

            app.MapControllers();

            app.Run();
        }
    }
}
