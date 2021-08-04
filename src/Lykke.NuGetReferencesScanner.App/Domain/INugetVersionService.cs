using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace Lykke.NuGetReferencesScanner.App.Domain
{
    public interface INugetVersionService
    {
        Task<IDictionary<string, SemanticVersion>> GetCurrentVersionsAsync(IEnumerable<string> packages,
            CancellationToken cancellationToken);
    }
}
