using System;
using System.Linq;
using Microsoft.Cci.MutableCodeModel;

namespace WeavR.Mutators
{
    public class ProcessedFlagMutator : Mutator
    {
        public override string Name
        {
            get { return "ProcessedFlag"; }
        }

        public override void Mutate(Assembly assembly)
        {
            Logger.LogInfo("Adding processed flag.");

            var processedInterface = new NamespaceTypeDefinition()
            {
                InternFactory = Host.InternFactory,
                ContainingUnitNamespace = assembly.UnitNamespaceRoot,
                Name = Host.NameTable.GetNameFor("ProcessedByWeavR"),
                IsAbstract = true,
                IsInterface = true,
                MangleName = false
            };
            assembly.AllTypes.Add(processedInterface);
            ((RootUnitNamespace)assembly.UnitNamespaceRoot).Members.Add(processedInterface);
        }
    }
}