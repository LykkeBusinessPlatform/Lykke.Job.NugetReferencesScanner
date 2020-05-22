using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.NuGetReferencesScanner.Domain
{
    internal interface IOrganizationScanner
    {
        Task ScanReposAsync(
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scnProgress);
    }
}
