using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using Microsoft.Cci.MutableCodeModel;

namespace WeavR
{
    public class Engine
    {
        private readonly ConcurrentDictionary<string, PdbReader> pdbReaders;
        private readonly IMetadataHost host;

        public Engine()
        {
            pdbReaders = new ConcurrentDictionary<string, PdbReader>();
            host = new PeReader.DefaultHost();
        }

        public void Process(string targetPath, string outpath)
        {
            Directory.CreateDirectory(outpath);

            var targetAssembly = ReadAssembly(targetPath);

            //targetAssembly = new QuackRewriter(host).Rewrite(targetAssembly);

            WriteAssembly(targetAssembly, targetPath, outpath);
        }

        private IAssembly ReadAssembly(string targetPath)
        {
            var targetAssembly = (IAssembly)host.LoadUnitFrom(targetPath);

            var pdbReader = GetPdbReader(targetAssembly);

            var decompiled = Decompiler.GetCodeModelFromMetadataModel(host, targetAssembly, pdbReader);

            return new CodeDeepCopier(host, pdbReader).Copy(decompiled);
        }

        private PdbReader GetPdbReader(IAssembly assembly)
        {
            string pdbFile = Path.ChangeExtension(assembly.Location, ".pdb");
            return pdbReaders.GetOrAdd(pdbFile, pdbf =>
                File.Exists(pdbf)
                ? new PdbReader(File.OpenRead(pdbf), host)
                : null);
        }

        private void WriteAssembly(IAssembly targetAssembly, string targetPath, string outpath)
        {
            var pdbReader = GetPdbReader(targetAssembly);

            var newAssemblyPath = Path.Combine(outpath, Path.GetFileName(targetPath));
            var newPdbPath = Path.ChangeExtension(newAssemblyPath, ".pdb");

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
}