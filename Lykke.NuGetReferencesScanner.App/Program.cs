using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostingContext, services) => services.AddHostedService<App>())
                .Build();

            await host.StartAsync();
        }
    }
}
