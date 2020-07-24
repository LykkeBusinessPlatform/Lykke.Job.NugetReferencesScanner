using System;
using NuGet.Versioning;

namespace Lykke.NuGetReferencesScanner.Domain
{
    public sealed class PackageReference
    {
        public string Name { get; }
        public IComparable Version { get; }



        public static PackageReference Parse(string name, string version)
        {
            version = version.Replace("*", "9999.9999");
            if (SemanticVersion.TryParse(version, out var ver))
            {
                return new PackageReference(name, ver);
            }
            if (System.Version.TryParse(version, out var ver2))
            {
                return new PackageReference(name, new SemanticVersion(ver2.Major, ver2.Minor, ver2.Build, ver2.Revision.ToString(), "NotSemVer!"));
            }
            return new PackageReference(name, new SemanticVersion(0, 0, 0, "UnableToParse"));
        }

        private PackageReference(string name, IComparable version)
        {
            Name = name;
            Version = version;
        }


        private bool Equals(PackageReference other)
        {
            return string.Equals(Name, other.Name) && Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PackageReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{Name} {Version}";
        }
    }
}
