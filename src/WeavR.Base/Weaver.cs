using System;
using System.Linq;
using Microsoft.Cci.MutableCodeModel;

namespace WeavR.Base
{
    public abstract class Weaver
    {
        public abstract void Process(Assembly assembly);
    }
}