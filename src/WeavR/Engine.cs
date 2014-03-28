using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;
using WeavR.Mutators;

namespace WeavR
{
    public class Engine
    {
        public class AssemblyDetails
        {
            private readonly string solutionDirectory;
            private readonly string projectDirectory;
            private readonly string targetPath;

            public AssemblyDetails(string solutionDirectory, string projectDirectory, string targetPath)
            {
                this.solutionDirectory = solutionDirectory;
                this.projectDirectory = projectDirectory;
                this.targetPath = targetPath;
            }

            public string SolutionDirectory { get { return solutionDirectory; } }
            public string ProjectDirectory { get { return projectDirectory; } }
            public string TargetPath { get { return targetPath; } }

            public bool Verify(ILoggerContext logger)
            {
                var result = true;

                if (!File.Exists(TargetPath))
                {
                    logger.LogError("Assembly '{0}' not found.", TargetPath);
                    result = false;
                }

                if (!Directory.Exists(ProjectDirectory))
                {
                    logger.LogError("Project directory '{0}' not found.", ProjectDirectory);
                    result = false;
                }

                return result;
            }
        }

        private readonly IMetadataHost host;
        private readonly ILoggerContext logger;

        private Engine(ILoggerContext logger, IMetadataHost host)
        {
            this.logger = logger;
            this.host = host;
        }

        private void Process(AssemblyDetails assemblyDetails, Assembly assembly)
        {
            logger.LogInfo("WeavR (version {0}) Executing", typeof(Engine).Assembly.GetName().Version);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (assembly.AllTypes.Any(t => t.Name.Value == "ProcessedByWeavR"))
                {
                    logger.LogWarning("Assembly already processed by WeavR.");
                    return;
                }

                var weaversConfigs = FindWeaverConfigs(assemblyDetails.ProjectDirectory)
                    .Select(s => XElement.Load(s))
                    .SelectMany(root => root.Descendants())
                    .ToList();

                var weavers = FindWeavers(assemblyDetails.SolutionDirectory);

                var weaverMutators = weaversConfigs
                    .Select(config => new { config, weaver = weavers.FirstOrDefault(w => Path.GetFileNameWithoutExtension(w) == config.Name.LocalName + ".WeavR") })
                    .WhereWithActions(a => a.weaver != null, null, a => logger.LogWarning(@"Could not find a weaver named '{0}'.", a.config.Name.LocalName)) //Error?
                    .Select(a => new WeaverMutator(logger, a.weaver))
                    .ToList();

                if (logger.HasLoggedError)
                    return;

                var mutators = new List<Mutator>();
                mutators.AddRange(weaverMutators);
                mutators.Add(new ProcessedFlagMutator());

                foreach (var mutator in mutators)
                {
                    mutator.Host = host;
                    mutator.Logger = logger.CreateSubContext(mutator.Name);

                    var weaverMutator = mutator as WeaverMutator;
                    if (weaverMutator != null)
                    {
                        var config = weaversConfigs.FirstOrDefault(c => c.Name == weaverMutator.Name);
                        if (config != null)
                            weaverMutator.Configure(config);
                    }

                    mutator.Mutate(assembly);
                }
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                logger.LogInfo("WeavR Finished {0}ms.", stopwatch.ElapsedMilliseconds);
            }
        }

        private IEnumerable<string> FindWeaverConfigs(string projectDirectory)
        {
            var foundConfigs = new List<string>();

            var projectConfigFilePath = Path.Combine(projectDirectory, "WeaversConfig.xml");
            if (File.Exists(projectConfigFilePath))
            {
                foundConfigs.Add(projectConfigFilePath);
                logger.LogInfo("Found path to weavers file '{0}'.", projectConfigFilePath);
            }

            if (foundConfigs.Count == 0)
            {
                var pathsSearched = string.Join("', '", projectConfigFilePath);
                logger.LogWarning("Could not find path to weavers file. Searched '{0}'.", pathsSearched);
            }

            return foundConfigs;
        }

        private IEnumerable<string> FindWeavers(string solutionDirectory)
        {
            var directories = new List<string>();
            directories.Add(Path.Combine(solutionDirectory, "Packages"));
            directories.Add(Path.Combine(solutionDirectory, "Tools"));
            directories.Add(GetPackagePath(Path.Combine(solutionDirectory, "nuget.config")));
            directories.Add(GetPackagePath(Path.Combine(solutionDirectory, ".nuget", "nuget.config")));
            return directories
                .Where(s => s != null)
                .WhereWithActions(s => Directory.Exists(s), null, s => logger.LogInfo("Skipped scanning '{0}' for weavers since it doesn't exist.", s))
                .SelectMany(d => Directory.EnumerateFiles(d, "*.WeavR.dll", SearchOption.AllDirectories));
        }

        private static string GetPackagePath(string nugetConfigPath)
        {
            if (File.Exists(nugetConfigPath))
            {
                XDocument xDocument;
                try
                {
                    xDocument = XDocument.Load(nugetConfigPath);
                }
                catch (XmlException)
                {
                    return null;
                }
                var repositoryPath = xDocument.Descendants("repositoryPath")
                    .Select(x => x.Value)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (repositoryPath != null)
                {
                    return Path.Combine(Path.GetDirectoryName(nugetConfigPath), repositoryPath);
                }
                repositoryPath = xDocument.Descendants("add")
                    .Where(x => (string)x.Attribute("key") == "repositoryPath")
                    .Select(x => x.Attribute("value"))
                    .Where(x => x != null)
                    .Select(x => x.Value)
                    .FirstOrDefault();
                if (repositoryPath != null)
                {
                    if (repositoryPath.StartsWith("$\\"))
                    {
                        return repositoryPath.Replace("$", Path.Combine(Path.GetDirectoryName(nugetConfigPath)));
                    }

                    return Path.Combine(Path.GetDirectoryName(nugetConfigPath), repositoryPath);
                }
            }
            return null;
        }

        public static void Process(ILoggerContext logger, AssemblyDetails assemblyDetails, string tempFolder = null)
        {
            if (!assemblyDetails.Verify(logger))
                return;

            tempFolder = tempFolder ?? Path.GetTempPath();

            string pdbFile = Path.ChangeExtension(assemblyDetails.TargetPath, ".pdb");

            var newAssemblyPath = Path.Combine(tempFolder, String.Format("{0}.weavr{1}", Path.GetFileNameWithoutExtension(assemblyDetails.TargetPath), Path.GetExtension(assemblyDetails.TargetPath)));
            var newPdbPath = File.Exists(pdbFile) ? Path.ChangeExtension(newAssemblyPath, ".pdb") : null;

            using (var host = new PeReader.DefaultHost())
            {
                var targetAssembly = (IAssembly)host.LoadUnitFrom(assemblyDetails.TargetPath);

                using (var pdbStream = newPdbPath != null ? File.OpenRead(pdbFile) : null)
                using (var pdbReader = newPdbPath != null ? new PdbReader(pdbStream, host) : null)
                {
                    var decompiled = Decompiler.GetCodeModelFromMetadataModel(host, targetAssembly, pdbReader);
                    decompiled = new CodeDeepCopier(host, pdbReader).Copy(decompiled);

                    var engine = new Engine(logger, host);
                    engine.Process(assemblyDetails, decompiled);

                    using (var peStream = File.Create(newAssemblyPath))
                    {
                        if (pdbReader == null)
                        {
                            PeWriter.WritePeToStream(decompiled, host, peStream);
                        }
                        else
                        {
                            using (var pdbWriter = new PdbWriter(newPdbPath, pdbReader))
                            {
                                PeWriter.WritePeToStream(decompiled, host, peStream, pdbReader, pdbReader, pdbWriter);
                            }
                        }
                    }
                }
            }

            File.Delete(assemblyDetails.TargetPath);
            File.Move(newAssemblyPath, assemblyDetails.TargetPath);

            if (!string.IsNullOrEmpty(newPdbPath))
            {
                File.Delete(pdbFile);
                File.Move(newPdbPath, pdbFile);
            }
        }
    }
}