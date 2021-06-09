using System;

namespace Lykke.NuGetReferencesScanner.Domain.Models
{
    public sealed class RepoInfo
    {
        public string Name { get; }
        public Uri Url { get; }

        private RepoInfo(string name, Uri url)
        {
            Name = name;
            Url = url;
        }

        public static RepoInfo Parse(string name, string url)
        {
            return new RepoInfo(name, new Uri(url));
        }

        private bool Equals(RepoInfo other)
        {
            return string.Equals(Name, other.Name) && Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RepoInfo info && Equals(info);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name != null ? Name.GetHashCode() : 0) * 397 ^ (Url != null ? Url.GetHashCode() : 0);
            }
        }
    }
}
