using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    internal sealed class PackageResolver
    {
        private const int MaxDependencyGraphDepth = 10;
        
        private readonly ILogger _logger;
        private readonly string _nugetCacheDirectory;
        private readonly PackageGraphCache _cache = new PackageGraphCache();

        public PackageResolver(ILogger logger)
        {
            _logger = logger;
            _nugetCacheDirectory = SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(null));
        }

        public IEnumerable<NuGetPackage> GetPackages()
        {
            return _cache.GetPackages();
        }
        
        public IPackageWithExecutableLoaded CreatePackage(PortableExecutableWrapper executable)
        {
            var name = executable.Name;
            var version = GetVersionFromFilepath(executable);
            
            if (!_cache.TryGetFirst(version, name, out var package))
            {
                return new PackageWithExecutableLoaded(PackageReferenceType.Unknown, version.ToString(), executable.Name, executable);
            }
            
            return new NuGetPackageWithExecutableLoaded(package, executable);
            
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
        
        public async Task CreatePackageGraph(Project project, NamespaceExclusionList exclusions, CancellationToken ct)
        {
            _logger.LogVerbose($"Resolving package dependency graph for project: {project.Name}");

            var (references, framework) = await GetExplicitPackageReferences(project, ct);
            
            _logger.LogVerbose($"Project target framework: {framework}");
            
            foreach (var reference in references)
            {
                var package = new NuGetPackage(PackageReferenceType.Explicit, reference.Version.ToString(), reference.Name, parent: null);
                
                _cache.Add(package);
                
                try
                {
                    var subgraph = GetPackageSubgraph(_cache, exclusions, framework, package);
                    
                    package.AddChildren(subgraph);
                }
                catch (InvalidOperationException exception)
                {
                    throw new InvalidOperationException($"Failed to resolve dependency graph of explicit package reference: {package.Name}", exception);
                }
            }
        }

        private static async Task<(List<PackageReference> references, NuGetFramework framework)> GetExplicitPackageReferences(Project project, CancellationToken ct)
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
                .Select(ParseToPackageReference)
                .ToList();

            var framework = ParseTargetFramework(project, document);

            return (references, framework);
            
            static PackageReference ParseToPackageReference(XElement elem)
            {
                var name = elem.Attribute("Include")?.Value ?? throw new InspectionException("Unable to resolve package name");
                var version = elem.Attribute("Version")?.Value ?? throw new InspectionException("Unable to resolve package version");

                if (!SemanticVersion.TryParse(version.Trim(), out var sVersion))
                {
                    throw new InspectionException($"Unable to parse the semantic version '{version}' of package: {name}");
                }

                return new PackageReference(name, sVersion);
            }

            static NuGetFramework ParseTargetFramework(Project project, XNode document)
            {
                var tf = document.XPathSelectElement("//TargetFramework");

                if (tf is null || tf.IsEmpty)
                {
                    throw new InspectionException($"Unable to determine target framework of project: {project.Name}");
                }
            
                return NuGetFramework.Parse(tf.Value);
            }
        }
        
        private List<NuGetPackage>? GetPackageSubgraph(PackageGraphCache cache, NamespaceExclusionList exclusions, NuGetFramework framework, NuGetPackage parent, int depth = 0)
        {
            if (depth > MaxDependencyGraphDepth)
            {
                throw new InvalidOperationException($"Max recursion depth reached: Exceeded depth of: {MaxDependencyGraphDepth}");
            }
            
            if (!TryGetCompatibleDependencyGroup(parent, framework, out var group))
            {
                return null;
            }

            var results = new List<NuGetPackage>();
            
            foreach (var dependency in group.Packages)
            {
                var name = dependency.Id;

                if (exclusions.IsExcluded(name))
                {
                    continue;
                }
                
                var version = dependency.VersionRange.MinVersion;

                if (!cache.TryGetFirst(version, name, out var package))
                {
                    package = new NuGetPackage(PackageReferenceType.Transient, version.ToString(), name, parent);
                    
                    package.AddChildren(GetPackageSubgraph(cache, exclusions, framework, package, depth + 1));
                    
                    cache.Add(package);
                }
                else
                {
                    package.AddParent(parent);
                }
                
                results.Add(package);
            }

            return results;
        }
        
        private bool TryGetCompatibleDependencyGroup(NuGetPackage package, NuGetFramework framework, [NotNullWhen(true)] out PackageDependencyGroup? group)
        {
            group = null;
            
            var file = CreateNuGetSpecFilename(package);

            try
            {
                var reader = new NuspecReader(file);

                group = reader
                    .GetDependencyGroups()
                    .FirstOrDefault(g => NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(framework, g.TargetFramework) || g.TargetFramework.IsAgnostic);
            }
            catch (IOException)
            {
                _logger.LogVerbose($"Unable to find NuGet package spec: {file}");

                return false;
            }
            
            return group != null;
        }
        
        private string CreateNuGetSpecFilename(NuGetPackage package)
        {
            return Path.Combine(_nugetCacheDirectory, package.Name, package.Version, $"{package.Name}.nuspec");
        }
    }
}
