using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

namespace WeavR
{
    public class Engine
    {
        private readonly IMetadataHost host;
        private readonly WeavRLogger logger;

        private Engine(WeavRLogger logger, IMetadataHost host)
        {
            this.logger = logger;
            this.host = host;
        }

        private void Process(string projectDirectory, Assembly assembly)
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

                var weaversConfigs = FindWeaverConfigs(projectDirectory)
                    .Select(s => XElement.Load(s))
                    .SelectMany(root => root.Descendants())
                    .ToList();

                // TODO modify assembly with weavers

                AddProcessedFlag(assembly);
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

        public IEnumerable<string> FindWeaverConfigs(string projectDirectory)
        {
            var foundConfigs = new List<string>();

            //var fodyDirConfigFilePath = Path.Combine(AssemblyLocation.CurrentDirectory(), "WeaversConfig.xml");
            //if (File.Exists(fodyDirConfigFilePath))
            //{
            //    ConfigFiles.Add(fodyDirConfigFilePath);
            //    Logger.LogInfo(string.Format("Found path to weavers file '{0}'.", fodyDirConfigFilePath));
            //}

            //var solutionConfigFilePath = Path.Combine(SolutionDirectoryPath, "WeaversConfig.xml");
            //if (File.Exists(solutionConfigFilePath))
            //{
            //    ConfigFiles.Add(solutionConfigFilePath);
            //    Logger.LogInfo(string.Format("Found path to weavers file '{0}'.", solutionConfigFilePath));
            //}

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

        private void AddProcessedFlag(Assembly assembly)
        {
            var processedInterface = new NamespaceTypeDefinition()
            {
                InternFactory = host.InternFactory,
                ContainingUnitNamespace = assembly.UnitNamespaceRoot,
                Name = host.NameTable.GetNameFor("ProcessedByWeavR"),
                IsAbstract = true,
                IsInterface = true,
                MangleName = false
            };
            assembly.AllTypes.Add(processedInterface);
            ((RootUnitNamespace)assembly.UnitNamespaceRoot).Members.Add(processedInterface);
        }

        public static void Process(WeavRLogger logger, string projectDirectory, string targetPath, string tempFolder = null)
        {
            if (!File.Exists(targetPath))
            {
                logger.LogError("Assembly '{0}' not found.", targetPath);
                return;
            }

            if (!Directory.Exists(projectDirectory))
            {
                logger.LogError("Project directory '{0}' not found.", projectDirectory);
                return;
            }

            tempFolder = tempFolder ?? Path.GetTempPath();

            string pdbFile = Path.ChangeExtension(targetPath, ".pdb");

            var newAssemblyPath = Path.Combine(tempFolder, String.Format("{0}.weavr{1}", Path.GetFileNameWithoutExtension(targetPath), Path.GetExtension(targetPath)));
            var newPdbPath = File.Exists(pdbFile) ? Path.ChangeExtension(newAssemblyPath, ".pdb") : null;

            using (var host = new PeReader.DefaultHost())
            {
                var targetAssembly = (IAssembly)host.LoadUnitFrom(targetPath);

                using (var pdbStream = newPdbPath != null ? File.OpenRead(pdbFile) : null)
                using (var pdbReader = newPdbPath != null ? new PdbReader(pdbStream, host) : null)
                {
                    var decompiled = Decompiler.GetCodeModelFromMetadataModel(host, targetAssembly, pdbReader);
                    decompiled = new CodeDeepCopier(host, pdbReader).Copy(decompiled);

                    var engine = new Engine(logger, host);
                    engine.Process(projectDirectory, decompiled);

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

            File.Delete(targetPath);
            File.Move(newAssemblyPath, targetPath);

            if (!string.IsNullOrEmpty(newPdbPath))
            {
                File.Delete(pdbFile);
                File.Move(newPdbPath, pdbFile);
            }
        }
    }
}