using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lykke.NuGetReferencesScanner.Domain.Models;

namespace Lykke.NuGetReferencesScanner.Services
{
    public class ProjectFileParser
    {
        private readonly Regex _regex;

        public ProjectFileParser(IEnumerable<string> referencePrefixes, bool isInclude)
        {
            string pattern = isInclude
                ? "<PackageReference\\s+Include\\s*=\\s*\\\"("
                 + string.Join('|', referencePrefixes.Select(s => $"{s}.+"))
                 + ")\\\"\\s+Version\\s*=\\s*\"(.+)\\\""
                : "<PackageReference\\s+Include\\s*=\\s*\\\"((?!("
                 + string.Join('|', referencePrefixes)
                 + ")).)(.+)\\\"";
            _regex = new Regex(pattern);
        }

        public IReadOnlyCollection<PackageReference> Parse(string projectFileContent)
        {
            var matches = _regex.Matches(projectFileContent);
            var result = matches
                .Select(m => ParseReferenceString(m.Value))
                .ToArray();

            return result;
        }

        private PackageReference ParseReferenceString(string referenceString)
        {
            var parts = referenceString.Split('"', StringSplitOptions.RemoveEmptyEntries);
            return PackageReference.Parse(parts[1], parts[3]);
        }
    }
}
