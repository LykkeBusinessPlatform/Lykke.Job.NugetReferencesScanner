using System.Threading.Tasks;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public interface IReferencesScanner
    {
        Task<ScanResult> GetScanResult();
        void Start();
    }
}
