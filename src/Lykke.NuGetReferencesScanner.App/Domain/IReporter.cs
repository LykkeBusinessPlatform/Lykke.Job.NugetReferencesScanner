using System;
using System.Collections.Generic;
using Lykke.NuGetReferencesScanner.Domain;
using NuGet.Versioning;

namespace Lykke.NuGetReferencesScanner.App.Domain
{
    public interface IReporter
    {
        public void Report(IReadOnlyCollection<Tuple<PackageReference, RepoInfo>> packagesInUse,
            IDictionary<string, SemanticVersion> currentVersions);
    }
}
