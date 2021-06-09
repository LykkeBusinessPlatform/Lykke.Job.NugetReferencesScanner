using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await BuildWebHost(args).RunAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        private static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel((ctx, opts) =>
                    {
                        webBuilder.UseUrls("http://*:5000");
                    });
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                })
                .Build();
    }
}
