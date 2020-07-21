using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain;
using Lykke.NuGetReferencesScanner.Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lykke.NuGetReferencesScanner.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostingContext, services) =>
                {
                    var configuration = hostingContext.Configuration;

                    services.Configure<GithubOptions>(configuration.GetSection("Github"));
                    services.Configure<BitBucketOptions>(configuration.GetSection("BitBucket"));

                    if (ScannerConfigured(GitHubScanner.OrganizationKeyEnvVar, configuration))
                        services.AddSingleton<IOrganizationScanner, GitHubScanner>(provider =>
                            new GitHubScanner(provider.GetRequiredService<IOptions<GithubOptions>>()));
                    if (ScannerConfigured(BitBucketScanner.AccountEnvVar, configuration))
                        services.AddSingleton<IOrganizationScanner, BitBucketScanner>(provider => 
                            new BitBucketScanner(provider.GetRequiredService<IOptions<BitBucketOptions>>()));

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
