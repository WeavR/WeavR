using System;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

namespace WeavR
{
    public class Engine
    {
        private readonly IMetadataHost host;
        private readonly StandardMessageLogger logger;

        private Engine(StandardMessageLogger logger, IMetadataHost host)
        {
            this.logger = logger;
            this.host = host;
        }

        private void Process(Assembly assembly)
        {
            if (assembly.AllTypes.Any(t => t.Name.Value == "ProcessedByWeavR"))
            {
                logger.LogAlreadyProcessedMessage();
                return;
            }

            // TODO modify assembly with weavers

            AddProcessedFlag(assembly);
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

        public static void Process(StandardMessageLogger logger, string targetPath, string tempFolder = null)
        {
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
                    engine.Process(decompiled);

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