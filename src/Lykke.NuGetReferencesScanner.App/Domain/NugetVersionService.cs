using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Lykke.NuGetReferencesScanner.App.Domain
{
    public class NugetVersionService : INugetVersionService
    {
        public async Task<IDictionary<string, SemanticVersion>> GetCurrentVersionsAsync(IEnumerable<string> packages,
            CancellationToken cancellationToken)
        {
            var logger = NullLogger.Instance;
            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            var result = new Dictionary<string, SemanticVersion>();
            
            foreach (var package in packages)
            {
                var versions = await resource.GetAllVersionsAsync(
                    package,
                    cache,
                    logger,
                    cancellationToken);
                var currentVersion = versions
                    .Where(v => !v.IsPrerelease)
                    .Max(getVersion => getVersion);
                
                result.Add(package, currentVersion);
            }

            return result;
        }
    }
}
