using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner.App
{
    public class App : IHostedService
    {
        private readonly IReferencesScanner _scanner;

        public App(IReferencesScanner scanner)
        {
            _scanner = scanner;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Start nuget scanner");
            var result = await _scanner.GetScanResult();
            Console.WriteLine("Stop nuget scanner");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
