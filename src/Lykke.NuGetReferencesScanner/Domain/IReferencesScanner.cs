namespace Lykke.NuGetReferencesScanner.Domain
{
    public interface IReferencesScanner
    {
        ScanResult GetScanResult();
        void Start();
    }
}
