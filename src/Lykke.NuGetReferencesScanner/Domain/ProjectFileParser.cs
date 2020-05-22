using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public static class ProjectFileParser
    {
        private static readonly List<string> _includePrefixes = new List<string>
        {
            "Lykke", "Falcon"
        };
        private static readonly List<string> _excludePrefixes = new List<string>
        {
            "Lykke", "Falcon"
        };

        private static readonly string IncludePattern;
        private static readonly string ExcludePattern;
        private static readonly Regex Regex;

        static ProjectFileParser()
        {
            IncludePattern = "<PackageReference\\s+Include\\s*=\\s*\\\"("
                 + string.Join('|', _includePrefixes.Select(s => $"{s}.+"))
                 + ")\\\"\\s+Version\\s*=\\s*\"(.+)\\\"";
            ExcludePattern = "<PackageReference\\s+Include\\s*=\\s*\\\"((?!("
                 + string.Join('|', _excludePrefixes)
                 + ")).)(.+)\\\"";
            Regex = new Regex(IncludePattern); // new Regex(ExcludePattern);
        }

        public static IReadOnlyCollection<PackageReference> Parse(string projectFileContent)
        {
            var matches = Regex.Matches(projectFileContent);
            var result = matches
                .Select(m => ParseReferenceString(m.Value))
                .ToArray();

            return result;
        }

        private static PackageReference ParseReferenceString(string referenceString)
        {
            var parts = referenceString.Split('"', StringSplitOptions.RemoveEmptyEntries);
            return PackageReference.Parse(parts[1], parts[3]);
        }
    }
}
