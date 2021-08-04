namespace Lykke.NuGetReferencesScanner.Domain.Abstractions
{
    internal interface IScanProgress
    {
        void SetRepoProgress(int processedReposCount);

        void UpdateRepoProgress();

        void UpdateProjectProgress();
    }
}
