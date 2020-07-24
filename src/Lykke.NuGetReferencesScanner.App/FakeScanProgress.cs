using Lykke.NuGetReferencesScanner.Domain;

namespace Lykke.NuGetReferencesScanner.App
{
    public class FakeScanProgress : IScanProgress
    {
        public void SetRepoProgress(int processedReposCount)
        {
        }

        public void UpdateRepoProgress()
        {
        }

        public void UpdateProjectProgress()
        {
        }
    }
}
