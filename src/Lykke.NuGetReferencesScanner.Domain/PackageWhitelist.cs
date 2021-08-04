using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public interface IPackageWhitelist
    {
        bool ShouldSkip(string repo);
    }

    public class PackageWhitelist : IPackageWhitelist
    {
        private List<string> _whitelist;
        public bool ShouldSkip(string repo) => _whitelist.Count > 0 && !_whitelist.Contains(repo);

        public PackageWhitelist(IConfiguration configuration)
        {
            _whitelist = configuration.GetSection("whitelist")
                .AsEnumerable()
                .Where(pair => pair.Value != null)
                .Select(pair => pair.Value)
                .ToList();
        }
    }
}
