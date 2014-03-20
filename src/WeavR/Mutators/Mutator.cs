using System;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

namespace WeavR.Mutators
{
    public abstract class Mutator
    {
        public IMetadataHost Host { get; set; }
        public LoggerContext Logger { get; set; }

        public abstract string Name { get; }

        public abstract void Mutate(Assembly assembly);
    }
}