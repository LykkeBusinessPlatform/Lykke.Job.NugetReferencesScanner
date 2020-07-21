using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostingContext, services) =>
                {
                    if(ScannerConfigured(GitHubScanner.OrganizationKeyEnvVar, hostingContext.Configuration))
                        services.AddSingleton<IOrganizationScanner, GitHubScanner>();
                    if(ScannerConfigured(BitBucketScanner.AccountEnvVar, hostingContext.Configuration))
                        services.AddSingleton<IOrganizationScanner, BitBucketScanner>();
                    
                    services.AddSingleton<IReferencesScanner, GitScanner>();
                    services.AddHostedService<App>();
                })
                .Build();

            await host.StartAsync();
        }

        private static bool ScannerConfigured(string scannerEnvVar, IConfiguration configuration)
        {
             var key = configuration[scannerEnvVar];
             return !string.IsNullOrWhiteSpace(key);
        }
    }
}
