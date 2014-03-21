using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Base;
using WeavR.Common;

namespace WeavR.Mutators
{
    public class WeaverMutator : Mutator
    {
        private readonly string assemblyName;
        private readonly Weaver weaver;

        public WeaverMutator(LoggerContext logger, string assemblyPath)
        {
            assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            assemblyName = assemblyName.Remove(assemblyName.Length - ".WeavR".Length);

            // TODO Load in separate app domain
            var weaverAssembly = System.Reflection.Assembly.LoadFile(assemblyPath);

            // TODO Just find all types that inherit from Weaver instead.
            var weaverType = weaverAssembly.GetType("ModuleWeaver");
            if (weaverType == null)
            {
                logger.LogError("Cannot find 'ModuleWeaver' type in weaver '{0}'.", assemblyName);
                return;
            }

            weaver = (Weaver)Activator.CreateInstance(weaverType);
            weaver.Logger = logger.CreateSubContext(assemblyName);
        }

        public void Configure(XElement config)
        {
            weaver.Configure(config);
        }

        public override string Name
        {
            get { return assemblyName; }
        }

        public override void Mutate(Assembly assembly)
        {
            weaver.Process(assembly);
        }
    }
}