using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public class GitScanner : IReferencesScanner, IScanProgress
    {
        private readonly TimeSpan _errorRetryDelay = TimeSpan.FromMinutes(20);
        private readonly IEnumerable<IOrganizationScanner> _organizationScanners;

        private ConcurrentDictionary<PackageReference, HashSet<RepoInfo>> _graph = new ConcurrentDictionary<PackageReference, HashSet<RepoInfo>>();
        private int _scannedProjectFilesCount;
        private int _foundReposCount;

        protected readonly Timer _timer;

        protected string _status;
        protected DateTime? _lastUpDateTime;

        public GitScanner(IEnumerable<IOrganizationScanner> organizationScanners)
        {
            _organizationScanners = organizationScanners;
            _timer = new Timer(_ => ScanAsync().GetAwaiter().GetResult());
        }

        public Task<ScanResult> GetScanResultAsync()
        {
            var flatResult = _graph.SelectMany(g => g.Value.Select(v => new Tuple<PackageReference, RepoInfo>(g.Key, v))).ToArray();
            var statString = $"Last update time {_lastUpDateTime}. Found {_foundReposCount} repositories, scanned {_scannedProjectFilesCount} projects.";

            return Task.FromResult(new ScanResult(_status, statString, flatResult));
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        public void SetRepoProgress(int processedReposCount)
        {
            _foundReposCount = processedReposCount;
        }

        public void UpdateRepoProgress()
        {
            ++_foundReposCount;
        }

        public void UpdateProjectProgress()
        {
            ++_scannedProjectFilesCount;
        }

        private async Task ScanAsync()
        {
            var graph = _graph.Count == 0
                ? _graph
                : new ConcurrentDictionary<PackageReference, HashSet<RepoInfo>>();

            _foundReposCount = 0;
            _scannedProjectFilesCount = 0;

            var scanStart = DateTime.UtcNow;
            _status = $"Scanning. Started at {scanStart:HH:mm:ss}";

            bool anyOrganizationProcessed = false;

            foreach (var organizationScanner in _organizationScanners)
            {
                try
                {
                    await organizationScanner.ScanReposAsync(graph, this);
                    anyOrganizationProcessed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            if (graph.Count > 0)
                _graph = graph;

            _lastUpDateTime = DateTime.UtcNow;
            Console.WriteLine($"Last scan took {_lastUpDateTime - scanStart}");

            if (anyOrganizationProcessed)
            {
                _timer.Change(TimeSpan.FromHours(2), Timeout.InfiniteTimeSpan);
                _status = "Idle";
            }
            else
            {
                _status = $"Couldn't process any organization - {DateTime.UtcNow:HH:mm:ss}. Restarting in {_errorRetryDelay} min.";
                _timer.Change(_errorRetryDelay, Timeout.InfiniteTimeSpan);
            }
        }
    }
}
