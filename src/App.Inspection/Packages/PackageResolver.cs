using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;

using NuGet.Packaging;
using NuGet.Frameworks;
using NuGet.Configuration;

using App.Inspection.Exceptions;
using App.Inspection.Executables;

namespace App.Inspection.Packages
{
    internal sealed class PackageResolver
    {
        private const int MaxDependencyGraphDepth = 10;
        
        private readonly ILogger _logger;
        private readonly string _nugetCacheDirectory;
        
        private readonly IDictionary<string, PackageReference> _references = new Dictionary<string, PackageReference>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<PackageReference, Package> _packages = new Dictionary<PackageReference, Package>();

        public PackageResolver(ILogger logger)
        {
            _logger = logger;
            _nugetCacheDirectory = SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(null));
        }

        public IEnumerable<Package> GetExplicitPackages()
        {
            foreach (var (_, reference) in _references)
            {
                if (!reference.IsExplicit)
                {
                    continue;
                }
                
                yield return new Package(PackageReferenceType.Explicit, reference.Name, reference.Version, null);
            }
        }
        
        public PackageExecutableLoaded CreatePackage(PortableExecutableWrapper executable)
        {
            var key = CreateKeyFromExecutable(executable);
            
            if (!_references.TryGetValue(key, out var reference))
            {
                return new PackageExecutableLoaded(PackageReferenceType.Unreferenced, executable.Name, executable);
            }

            var parent = GetParentPackage(reference);
            var type = parent is null
                ? PackageReferenceType.Explicit
                : PackageReferenceType.Transient;
            
            return new PackageExecutableLoaded(type, reference.Name, reference.Version, executable, parent);

            static string CreateKeyFromExecutable(PortableExecutableWrapper executable)
            {
                var name = executable.Name;
                var version = GetVersionFromFilepath(executable);

                return $"{name}:{version}";
            }
            
            static string GetVersionFromFilepath(PortableExecutableWrapper executable)
            {
                // This is a hack! Sorry future me :(
                // The path to the DLL of a NuGet package always ends with /lib/<target framework>/<package-id>.dll, so
                // if we split on directory separators and take the 4th last segment that should always be the
                // version... assuming the NuGet package structure remains stable.
                var segments = executable.Filepath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var version = segments[^4];

                return version;
            }
        }
        
        public async Task LoadPackageDependencyGraph(Project project, NamespaceExclusionList exclusions, CancellationToken ct)
        {
            _logger.LogVerbose($"Resolving package dependency graph for project: {project.Name}");

            var (packages, framework) = await GetExplicitPackageReferences(project, ct);

            foreach (var package in packages)
            {
                try
                {
                    AddPackageReference(package);
                    
                    ResolveDependencyGraph(exclusions, framework, package);
                }
                catch (InvalidOperationException exception)
                {
                    throw new InvalidOperationException($"Failed to resolve dependency graph of explicit package reference: {package.Name}", exception);
                }
            }
        }

        private static async Task<(List<PackageReference> packages, NuGetFramework framework)> GetExplicitPackageReferences(Project project, CancellationToken ct)
        {
            var file = project.FilePath;

            if (file is null)
            {
                throw new InspectionException($"Could not determine project file (.csproj file) for project: {project.Name}");
            }

            var content = await File.ReadAllTextAsync(file, ct);
            var document = XDocument.Parse(content);
            var references = document.XPathSelectElements("//PackageReference")
                .Where(elem => elem.HasAttributes)
                .Select(elem => PackageReference.FromXElement(elem))
                .ToList();

            var tf = document.XPathSelectElement("//TargetFramework");

            if (tf is null || tf.IsEmpty)
            {
                throw new InspectionException($"Unable to determine target framework of project: {project.Name}");
            }
            
            var framework = NuGetFramework.Parse(tf.Value);

            return (references, framework);
        }

        private Package? GetParentPackage(PackageReference reference)
        {
            if (reference.Parent is null)
            {
                return null;
            }
            
            if (_packages.TryGetValue(reference, out var package))
            {
                return package;
            }
            
            package = new Package(PackageReferenceType.Transient, reference.Parent.Name, reference.Parent.Version, GetParentPackage(reference.Parent));

            _packages.Add(reference, package);
            
            return package;
        }

        private void AddPackageReference(PackageReference package)
        {
            var key = $"{package.Name}:{package.Version}";

            if (!_references.ContainsKey(key))
            {
                _references.Add(key, package);                
            }
        }

        private void ResolveDependencyGraph(NamespaceExclusionList exclusions, NuGetFramework framework, PackageReference parent, int depth = 0)
        {
            if (depth > MaxDependencyGraphDepth)
            {
                throw new InvalidOperationException($"Max recursion depth reached: Exceeded depth of: {MaxDependencyGraphDepth}");
            }

            var file = CreateNuGetSpecFilename(parent, _nugetCacheDirectory);

            if (!File.Exists(file))
            {
                _logger.LogWarning($"Unable to find NuGet package spec: {file}");

                return;
            }
            
            var reader = new NuspecReader(file);
            
            var compatibleDependencyGroup = reader
                .GetDependencyGroups()
                .FirstOrDefault(group => NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(framework, group.TargetFramework) || group.TargetFramework.IsAgnostic);

            if (compatibleDependencyGroup is null)
            {
                return;
            }
            
            foreach (var dependency in compatibleDependencyGroup.Packages)
            {
                var name = dependency.Id;

                if (exclusions.IsExcluded(name))
                {
                    continue;
                }
                
                var version = dependency.VersionRange.MinVersion;
                var package = new PackageReference(name, version, parent);
                
                AddPackageReference(package);
                
                ResolveDependencyGraph(exclusions, framework, package, depth + 1);
            }
        }

        private static string CreateNuGetSpecFilename(PackageReference package, string directory)
        {
            return Path.Combine(directory, package.Name, package.Version.ToString(), $"{package.Name}.nuspec");
        }
    }
}
