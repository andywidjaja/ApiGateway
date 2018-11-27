using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("configuration.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration)
                .AddDelegatingHandler<ShapeManagerRequestHandler>();
                //.AddCacheManager(x =>
                //{
                //    x.WithMicrosoftLogging(log =>
                //        {
                //            log.AddConsole(LogLevel.Debug);
                //        })
                //        .WithDictionaryHandle();
                //});

            services.Configure<PtLoggerSettings>(Configuration.GetSection("PtLoggerSettings"));
            services.Configure<RouteSettings>(Configuration.GetSection("RouteSettings"));

            services.AddSingleton<IPtLogger, PtLogger>();
            services.AddTransient<IRestClientFactory, RestClientFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            app.UseOcelot().Wait();
        }

        public IConfigurationRoot Configuration { get; }
    }
}