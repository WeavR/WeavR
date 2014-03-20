using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Base;

namespace WeavR.Mutators
{
    public class WeaverMutator : Mutator
    {
        private readonly string assemblyName;
        private readonly Weaver weaver;

        public WeaverMutator(string assemblyName)
        {
            this.assemblyName = assemblyName;
            weaver = (Weaver)Activator.CreateInstance(assemblyName, "ModuleWeaver").Unwrap();
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
            weaver.Process(Logger, assembly);
        }
    }
}