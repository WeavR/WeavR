using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;
using WeavR.Common;

namespace WeavR.Base
{
    public abstract class Weaver
    {
        public LoggerContext Logger { get; set; }

        public abstract void Configure(XElement config);

        public abstract void Process(Assembly assembly);
    }
}