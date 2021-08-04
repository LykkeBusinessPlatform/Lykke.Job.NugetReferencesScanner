using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain;

namespace Lykke.NuGetReferencesScanner.App.Domain
{
    public class ConsoleGitScanner : IReferencesScanner
    {
        private readonly IEnumerable<IOrganizationScanner> _organizationScanners;

        public ConsoleGitScanner(IEnumerable<IOrganizationScanner> organizationScanners)
        {
            _organizationScanners = organizationScanners;
        }

        public async Task<ScanResult> GetScanResultAsync()
        {
            var graph = await GetGraph();

            return new ScanResult(graph);
        }

        private async Task<ConcurrentDictionary<PackageReference, HashSet<RepoInfo>>> GetGraph()
        {
            var graph = new ConcurrentDictionary<PackageReference, HashSet<RepoInfo>>();
            
            foreach (var organizationScanner in _organizationScanners)
            {
                try
                {
                    await organizationScanner.ScanReposAsync(graph, new FakeScanProgress());
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            
            return graph;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
