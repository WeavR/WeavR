using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Base;

namespace WeavR.Mutators
{
    public abstract class WeaverMutator : Mutator
    {
        public abstract void Configure(XElement config);
    }

    public class WeaverMutator<T> : WeaverMutator where T : Weaver
    {
        private readonly T weaver;

        public WeaverMutator()
        {
            // TODO Spin up weaver in new appdomain
            weaver = Activator.CreateInstance<T>();
        }

        public override void Configure(XElement config)
        {
            weaver.Configure(config);
        }

        public override string Name
        {
            get { return typeof(T).Name; }
        }

        public override void Mutate(Assembly assembly)
        {
            weaver.Process(assembly);
        }
    }
}