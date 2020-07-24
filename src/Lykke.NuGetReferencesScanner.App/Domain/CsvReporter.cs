using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Lykke.NuGetReferencesScanner.Domain;
using NuGet.Versioning;

namespace Lykke.NuGetReferencesScanner.App.Domain
{
    public class CsvReporter : IReporter
    {
        public void Report(IReadOnlyCollection<Tuple<PackageReference, RepoInfo>> packagesInUse,
            IDictionary<string, SemanticVersion> currentVersions)
        {
            try
            {
                var records = packagesInUse
                    .Where(tuple => currentVersions[tuple.Item1.Name] != null)
                    .OrderBy(tuple => tuple.Item1.Name)
                    .ThenBy(tuple => tuple.Item1.Version)
                    .ThenBy(tuple => tuple.Item2.Name)
                    .Select(tuple =>
                        {
                            var currentVersion = currentVersions[tuple.Item1.Name];

                            return new
                            {
                                PackageName = tuple.Item1.Name,
                                CurrentVersion = currentVersion.ToString(),
                                UsedVersion = tuple.Item1.Version.ToString(),
                                Outdated = (currentVersion > (SemanticVersion) tuple.Item1.Version).ToString().ToLower(),
                                Repo = tuple.Item2.Name,
                                Url = tuple.Item2.Url.ToString(),
                            };
                        }
                    ).ToList();

                using var writer = new StreamWriter("report.csv");
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                });

                csv.WriteRecords(records);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
