using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.App.Domain;
using Lykke.NuGetReferencesScanner.Domain;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner.App
{
    public class App : IHostedService
    {
        private readonly IReferencesScanner _scanner;
        private readonly INugetVersionService _nugetVersionService;
        private readonly IReporter _reporter;

        public App(IReferencesScanner scanner, INugetVersionService nugetVersionService, IReporter reporter)
        {
            _scanner = scanner;
            _nugetVersionService = nugetVersionService;
            _reporter = reporter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Start nuget scanner");
            var scanResult = await _scanner.GetScanResultAsync();

            var packages = scanResult.Graph.Keys.Select(reference => reference.Name).Distinct();
            
            Console.WriteLine("Start getting current versions");
            var currentVersions = await _nugetVersionService.GetCurrentVersions(packages, cancellationToken);
            
            Console.WriteLine("Start csv export");
            _reporter.Report(scanResult.Data, currentVersions);

            Console.WriteLine("Stop nuget scanner");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
