using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;

namespace WeavR.Common
{
    public abstract class Weaver
    {
        public LoggerContext Logger { get; set; }

        public abstract void Configure(XElement config);

        public abstract void Process(Assembly assembly);
    }
}