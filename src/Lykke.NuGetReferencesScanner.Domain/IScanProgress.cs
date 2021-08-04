namespace Lykke.NuGetReferencesScanner.Domain
{
    public interface IScanProgress
    {
        void SetRepoProgress(int processedReposCount);

        void UpdateRepoProgress();

        void UpdateProjectProgress();
    }
}
