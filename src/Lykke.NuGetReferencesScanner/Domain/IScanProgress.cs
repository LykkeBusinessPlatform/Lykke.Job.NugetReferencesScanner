namespace Lykke.NuGetReferencesScanner.Domain
{
    internal interface IScanProgress
    {
        void SetRepoProgress(int processedReposCount);

        void UpdateRepoProgress();

        void UpdateProjectProgress();
    }
}
