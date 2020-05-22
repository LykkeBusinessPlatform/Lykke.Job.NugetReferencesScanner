using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

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

        private static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://*:5000")
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .Build();
    }
}
