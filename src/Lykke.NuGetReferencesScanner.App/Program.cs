using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.App.Domain;
using Lykke.NuGetReferencesScanner.Domain;
using Lykke.NuGetReferencesScanner.Domain.Options;
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
                    var configuration = hostingContext.Configuration;

                    var githubSection = configuration.GetSection(GitHubScanner.ConfigurationSection);
                    var bitBucketSection = configuration.GetSection(BitBucketScanner.ConfigurationSection);

                    if (githubSection.Exists())
                    {
                        services.Configure<GithubOptions>(githubSection);
                        services.AddSingleton<IOrganizationScanner, GitHubScanner>();
                    }

                    if (bitBucketSection.Exists())
                    {
                        services.Configure<BitBucketOptions>(bitBucketSection);
                        services.AddSingleton<IOrganizationScanner, BitBucketScanner>();
                    }

                    services.AddSingleton<IParserModeProvider, ParserModeProvider>();
                    services.AddSingleton<INugetVersionService, NugetVersionService>();
                    services.AddSingleton<IReporter, CsvReporter>();
                    services.AddSingleton<IReferencesScanner, ConsoleGitScanner>();
                    services.AddHostedService<App>();
                })
                .Build();

            await host.StartAsync();
        }
    }
}
