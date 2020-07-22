using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public sealed class ScanResult
    {
        public string Status { get; }
        public string Statistics { get; }

        public IReadOnlyDictionary<PackageReference, HashSet<RepoInfo>> Graph { get; }
        public IReadOnlyCollection<Tuple<PackageReference, RepoInfo>> Data { get; }

        public ScanResult(string status, string statistics, IReadOnlyCollection<Tuple<PackageReference, RepoInfo>> data)
        {
            Status = status;
            Statistics = statistics;
            Data = data;
        }
        
        public ScanResult(IReadOnlyDictionary<PackageReference, HashSet<RepoInfo>> graph)
        {
            Graph = graph;
            Data = graph
                .SelectMany(g => g.Value.Select(v => new Tuple<PackageReference, RepoInfo>(g.Key, v))).ToArray();
        }
    }
}
