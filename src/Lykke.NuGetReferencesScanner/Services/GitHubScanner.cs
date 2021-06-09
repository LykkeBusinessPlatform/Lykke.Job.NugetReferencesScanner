using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.NuGetReferencesScanner.Domain.Abstractions;
using Lykke.NuGetReferencesScanner.Domain.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Octokit;
using Octokit.Internal;

namespace Lykke.NuGetReferencesScanner.Services
{
    internal class GitHubScanner : IOrganizationScanner
    {
        private readonly string AppName = PlatformServices.Default.Application.ApplicationName;

        private readonly ProjectFileParser _projectFileParser;
        private readonly GitHubClient _client;
        private readonly HashSet<string> _solutions = new HashSet<string>();
        private readonly string _organization;
        private readonly string _token;

        public GitHubScanner(
            ProjectFileParser projectFileParser,
            string organization,
            string accessToken)
        {
            _projectFileParser = projectFileParser;
            _token = accessToken ?? throw new InvalidOperationException(
                $"Access token can't be empty. For unauthenticated requests rate limit = 60 calls per hour!");
            _organization = organization ?? throw new ArgumentNullException(nameof(organization));

            var proxy = new WebProxy();
            var connection = new Connection(
                new ProductHeaderValue(AppName),
                new HttpClientAdapter(() => HttpMessageHandlerFactory.CreateDefault(proxy)));

            _client = new GitHubClient(new ProductHeaderValue(AppName))
            {
                Credentials = new Credentials(_token),
            };
        }

        public  async Task ScanReposAsync(
            ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> graph,
            IScanProgress scanProgress)
        {
            _solutions.Clear();

            var scr = new SearchCodeRequest("PackageReference")
            {
                Organizations = new List<string> { _organization },
                Extensions = new List<string> { "csproj" },

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
                    try
                    {
                        bool wasAdded = _solutions.Add(item.Repository.Name);
                        if (wasAdded)
                            scanProgress.UpdateRepoProgress();

                        await IndexProjectAsync(item, graph, scanProgress);

                        // rate limit - 5000 per hour
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Error: {ex.Message}");
                    }
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
            var nugetRefs = _projectFileParser.Parse(projectContent[0].Content);

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
