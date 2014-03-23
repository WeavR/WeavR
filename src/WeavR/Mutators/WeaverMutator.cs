using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

namespace WeavR.Mutators
{
    public class WeaverMutator : Mutator
    {
        private readonly string weaverName;
        private readonly Weaver weaver;

        public WeaverMutator(LoggerContext logger, string assemblyPath)
        {
            weaverName = Path.GetFileNameWithoutExtension(assemblyPath);
            weaverName = weaverName.Remove(weaverName.Length - ".WeavR".Length);

            // TODO Load in separate app domain
            var weaverAssembly = System.Reflection.Assembly.LoadFile(assemblyPath);

            // TODO Just find all types that inherit from Weaver instead.
            var weaverType = weaverAssembly.GetType("ModuleWeaver");
            if (weaverType == null)
            {
                logger.LogError("Cannot find 'ModuleWeaver' type in weaver '{0}'.", weaverName);
                return;
            }

            weaver = (Weaver)Activator.CreateInstance(weaverType);
            weaver.Logger = logger.CreateSubContext(weaverName);
        }

        public void Configure(XElement config)
        {
            weaver.Configure(config);
        }

        public override string Name
        {
            get { return weaverName; }
        }

        public override void Mutate(Assembly assembly)
        {
            weaver.Process(assembly);
        }
    }
}