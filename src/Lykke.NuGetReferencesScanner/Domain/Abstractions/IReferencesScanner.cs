using Lykke.NuGetReferencesScanner.Domain.Models;

namespace Lykke.NuGetReferencesScanner.Domain.Abstractions
{
    public interface IReferencesScanner
    {
        ScanResult GetScanResult();
        void Start();
    }
}
