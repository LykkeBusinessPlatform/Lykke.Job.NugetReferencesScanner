using System;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public class ConfigurationValueMissingException : Exception
    {
        public ConfigurationValueMissingException(string sectionName, string valueName)
            : base($"Configuration value {sectionName}__{valueName} missing")
        {
        }
        
        public ConfigurationValueMissingException(string sectionName, string valueName, string message)
            : base($"Configuration value {sectionName}__{valueName} missing. {message}")
        {
        }
    }
}
