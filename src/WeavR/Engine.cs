using System;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;

using Microsoft.Cci.MutableCodeModel;

namespace WeavR
{
    public class Engine
    {
        private readonly IMetadataHost host;

        private Engine(IMetadataHost host)
        {
            this.host = host;
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

        public static void Process(string targetPath)
        {
            string pdbFile = Path.ChangeExtension(targetPath, ".pdb");

            var newAssemblyPath = Path.GetTempFileName();
            var newPdbPath = File.Exists(pdbFile) ? Path.GetTempFileName() : null;

            if (newPdbPath != null)
            {
                File.Delete(newPdbPath);
                newPdbPath = Path.ChangeExtension(newPdbPath, ".pdb");
            }

            using (var host = new PeReader.DefaultHost())
            {
                var targetAssembly = (IAssembly)host.LoadUnitFrom(targetPath);

                using (var pdbStream = newPdbPath != null ? File.OpenRead(pdbFile) : null)
                using (var pdbReader = newPdbPath != null ? new PdbReader(pdbStream, host) : null)
                {
                    var decompiled = Decompiler.GetCodeModelFromMetadataModel(host, targetAssembly, pdbReader);
                    decompiled = new CodeDeepCopier(host, pdbReader).Copy(decompiled);

                    // TODO modify assembly with weavers

                    var engine = new Engine(host);
                    engine.AddProcessedFlag(decompiled);

                    using (var peStream = File.Create(newAssemblyPath))
                    {
                        if (pdbReader == null)
                        {
                            PeWriter.WritePeToStream(targetAssembly, host, peStream);
                        }
                        else
                        {
                            using (var pdbWriter = new PdbWriter(newPdbPath, pdbReader))
                            {
                                PeWriter.WritePeToStream(targetAssembly, host, peStream, pdbReader, pdbReader, pdbWriter);
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