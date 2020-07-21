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

                    var githubSection = configuration.GetSection(GitHubScanner.ConfigurationSection);
                    var bitBucketSection = configuration.GetSection(BitBucketScanner.ConfigurationSection);

                    if (githubSection.Exists())
                    {
                        services.Configure<GithubOptions>(githubSection);
                        services.AddSingleton<IOrganizationScanner, GitHubScanner>(provider =>
                            new GitHubScanner(provider.GetRequiredService<IOptions<GithubOptions>>()));
                    }

                    if (bitBucketSection.Exists())
                    {
                        services.Configure<BitBucketOptions>(bitBucketSection);
                        services.AddSingleton<IOrganizationScanner, BitBucketScanner>(provider => 
                            new BitBucketScanner(provider.GetRequiredService<IOptions<BitBucketOptions>>()));
                    }

                    services.AddSingleton<IReferencesScanner, ConsoleGitScanner>();
                    services.AddHostedService<App>();
                })
                .Build();

            await host.StartAsync();
        }
    }
}
