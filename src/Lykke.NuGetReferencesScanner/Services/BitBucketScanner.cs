using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain.Abstractions;
using Lykke.NuGetReferencesScanner.Domain.Models;
using SharpBucket;
using SharpBucket.V2;
using SharpBucket.V2.Pocos;

namespace Lykke.NuGetReferencesScanner.Services
{
    internal class BitBucketScanner : IOrganizationScanner
    {
        private readonly ProjectFileParser _projectFileParser;
        private readonly SharpBucketV2 _client;
        private readonly string _bbAccount;
        private readonly Dictionary<string, RepoInfo> _reposCache = new Dictionary<string, RepoInfo>();

        private readonly List<string> _skipRepos = new List<string>
        {
        };

        public BitBucketScanner(
            ProjectFileParser projectFileParser,
            string account,
            string key,
            string secretKey)
        {
            _projectFileParser = projectFileParser;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentNullException(nameof(secretKey));
            _bbAccount = account ?? throw new InvalidOperationException(nameof(account));

            _client = new SharpBucketV2();
            _client.OAuth2ClientCredentials(key, secretKey);
        }

        public Task ScanReposAsync(
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            scanProgress.SetRepoProgress(_reposCache.Count);

            var teamResource = _client.TeamsEndPoint().TeamResource(_bbAccount);
            var searchResults = teamResource.EnumerateSearchCodeSearchResults("PackageReference");
            foreach (var searchResult in searchResults)
            {
                IndexProject(searchResult, graph, scanProgress);
            }

            return Task.CompletedTask;
        }

        private void IndexProject(
            SearchCodeSearchResult projectFile,
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            var retryCount = 0;
            while (retryCount < 10)
            {
                try
                {
                    IndexProject(projectFile.file, graph, scanProgress);
                    break;
                }
                catch (BitbucketException)
                {
                    ++retryCount;
                    //To reduce calls rate limit
                    Thread.Sleep(TimeSpan.FromSeconds(5 * retryCount));
                }
            }
        }

        private void IndexProject(
            SrcFileInfo searchFile,
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            var fileName = Path.GetFileName(searchFile.path);
            if (!fileName.EndsWith(".csproj"))
                return;

            var repoSlug = ExtractSlugFromUrl(searchFile.links.self.href);
            if (_skipRepos.Contains(repoSlug))
            {
                Console.WriteLine($"Skipped {repoSlug}");
                return;
            }

            var repoResource = _client.RepositoriesEndPoint().RepositoryResource(_bbAccount, repoSlug);
            if (!_reposCache.TryGetValue(repoSlug, out var repo))
            {
                var repoInfo = repoResource.GetRepository();
                var fileLink = $"{repoInfo.links.html.href}/src/master/{searchFile.path}";
                repo = RepoInfo.Parse(repoInfo.slug, fileLink);
                _reposCache.Add(repoSlug, repo);
                scanProgress.UpdateRepoProgress();
            }

            var projectContent = repoResource.SrcResource().GetFileContent(searchFile.path);
            var nugetRefs = _projectFileParser.Parse(projectContent);

            foreach (var nugetRef in nugetRefs)
            {
                if (!graph.TryGetValue(nugetRef, out var repoInfos))
                    repoInfos = new HashSet<RepoInfo>();

                repoInfos.Add(repo);
                graph[nugetRef] = repoInfos;
            }

            scanProgress.UpdateProjectProgress();

            Console.WriteLine($"Processed {fileName} from {repo.Name}");
        }

        private string ExtractSlugFromUrl(string url)
        {
            var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 1; i < parts.Length; i++)
            {
                if (parts[i] == "src")
                    return parts[i - 1];
            }

            throw new ArgumentException($"Coudln't extract repo sluf from url {url}");
        }
    }
}
