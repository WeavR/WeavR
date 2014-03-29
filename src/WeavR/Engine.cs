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
using WeavR.Mutators;

namespace WeavR
{
    public class Engine
    {
        private readonly IMetadataHost host;
        private readonly ILoggerContext logger;

        private Engine(ILoggerContext logger, IMetadataHost host)
        {
            this.logger = logger;
            this.host = host;
        }

        private void Process(ProjectDetails assemblyDetails, Assembly assembly)
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

                var weaverFinder = new WeaverFinder(logger, assemblyDetails);

                var weaversConfigs = weaverFinder.FindWeaverConfigs()
                    .Select(s => XElement.Load(s))
                    .SelectMany(root => root.Descendants())
                    .ToList();

                var weavers = weaverFinder.FindWeavers();

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

        public static void Process(ILoggerContext logger, ProjectDetails assemblyDetails, string tempFolder = null)
        {
            if (!assemblyDetails.Verify(logger))
                return;

            tempFolder = tempFolder ?? Path.GetTempPath();

            string pdbFile = Path.ChangeExtension(assemblyDetails.AssemblyPath, ".pdb");

            var newAssemblyPath = Path.Combine(tempFolder, String.Format("{0}.weavr{1}", Path.GetFileNameWithoutExtension(assemblyDetails.AssemblyPath), Path.GetExtension(assemblyDetails.AssemblyPath)));
            var newPdbPath = File.Exists(pdbFile) ? Path.ChangeExtension(newAssemblyPath, ".pdb") : null;

            using (var host = new PeReader.DefaultHost())
            {
                var targetAssembly = (IAssembly)host.LoadUnitFrom(assemblyDetails.AssemblyPath);

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

            File.Delete(assemblyDetails.AssemblyPath);
            File.Move(newAssemblyPath, assemblyDetails.AssemblyPath);

            if (!string.IsNullOrEmpty(newPdbPath))
            {
                File.Delete(pdbFile);
                File.Move(newPdbPath, pdbFile);
            }
        }
    }
}