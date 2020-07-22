using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain.Options;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public class GitHubScanner : IOrganizationScanner
    {
        private readonly IParserModeProvider _parserModeProvider;
        private readonly GitHubClient _client;
        private readonly HashSet<string> _solutions = new HashSet<string>();
        private readonly string _organization;

        public const string ConfigurationSection = "Github";

        public GitHubScanner(IOptions<GithubOptions> options, IParserModeProvider parserModeProvider) : this(
            options.Value.Organization, options.Value.ApiKey)
        {
            _parserModeProvider = parserModeProvider;
        }

        private GitHubScanner(string organization, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ConfigurationValueMissingException(ConfigurationSection, nameof(apiKey),
                    "For unauthenticated requests rate limit = 60 calls per hour!");
            if (string.IsNullOrWhiteSpace(organization))
                throw new ConfigurationValueMissingException(ConfigurationSection, nameof(organization));

            _organization = organization;

            _client = new GitHubClient(new ProductHeaderValue("MyAmazingApp2"),
                new InMemoryCredentialStore(new Credentials(apiKey)));
        }

        public async Task ScanReposAsync(
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            _solutions.Clear();

            var scr = new SearchCodeRequest("PackageReference Lykke")
            {
                Organization = _organization, Extensions = new List<string> {"csproj"}
            };
            var searchResult = await _client.Search.SearchCode(scr);
            var totalProjectsCount = searchResult.TotalCount;

            for (int i = 0; i < totalProjectsCount; i += 100)
            {
                var pageNumber = i / 100;
                scr.Page = pageNumber;

                searchResult = await _client.Search.SearchCode(scr);

                Console.WriteLine($"Page {pageNumber} received {searchResult.Items.Count}");

                foreach (var item in searchResult.Items)
                {
                    bool wasAdded = _solutions.Add(item.Repository.Name);
                    if (wasAdded)
                        scanProgress.UpdateRepoProgress();

                    await IndexProjectAsync(item, graph, scanProgress);

                    // rate limit - 5000 per hour
                    await Task.Delay(500);
                }
            }
        }

        private async Task IndexProjectAsync(
            SearchCode repoInfo,
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            var projectContent = await _client.Repository.Content.GetAllContents(repoInfo.Repository.Id, repoInfo.Path);
            var repo = RepoInfo.Parse(repoInfo.Repository.Name, repoInfo.HtmlUrl);
            var nugetRefs = ProjectFileParser.Parse(projectContent[0].Content, _parserModeProvider.GetParserMode());

            Console.WriteLine($"Repo name {repoInfo.Repository.Name} file name {repoInfo.Name}");

            foreach (var nugetRef in nugetRefs)
            {
                if (!graph.TryGetValue(nugetRef, out var repoInfos))
                    repoInfos = new HashSet<RepoInfo>();

                repoInfos.Add(repo);
                graph[nugetRef] = repoInfos;
            }

            scanProgress.UpdateProjectProgress();
        }
    }
}
