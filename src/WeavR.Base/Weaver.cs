using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Cci.MutableCodeModel;

namespace WeavR.Base
{
    public abstract class Weaver
    {
        public abstract void Configure(XElement config);

        public abstract void Process(Assembly assembly);
    }
}