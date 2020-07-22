using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public class ParserModeProvider : IParserModeProvider
    {
        private Dictionary<string, ProjectFileParserMode> modes = new Dictionary<string, ProjectFileParserMode>()
        {
            {"all", ProjectFileParserMode.All},
            {"include", ProjectFileParserMode.Include},
            {"exclude", ProjectFileParserMode.Exclude},
        };

        private string _mode;

        public ParserModeProvider(IConfiguration configuration)
        {
            _mode = configuration["mode"];
        }
        
        public ProjectFileParserMode GetParserMode()
        {
            if (string.IsNullOrEmpty(_mode)) return ProjectFileParserMode.All;
            if(!modes.ContainsKey(_mode)) throw new ArgumentException("Must provide correct parser mode: all, include or exclude");
            return modes[_mode];
        }
    }
}
