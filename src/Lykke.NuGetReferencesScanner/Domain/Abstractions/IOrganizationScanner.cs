using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain.Models;

namespace Lykke.NuGetReferencesScanner.Domain.Abstractions
{
    internal interface IOrganizationScanner
    {
        Task ScanReposAsync(
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scnProgress);
    }
}
