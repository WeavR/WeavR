using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

public class ModuleWeaver : Weaver
{
    public override void Configure(XElement config)
    {
        Logger.LogInfo("Doing Configure");
    }

    public override void Process(Assembly assembly)
    {
        Logger.LogInfo("Processing Assembly");
    }
}