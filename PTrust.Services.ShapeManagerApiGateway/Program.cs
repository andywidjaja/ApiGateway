using System.Net;
using System.Threading;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(500, 500);

            ServicePointManager.DefaultConnectionLimit = 2000;

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}