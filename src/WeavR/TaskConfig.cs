using System;
using System.Linq;

namespace WeavR
{
    [Serializable]
    public class TaskConfig
    {
        public string SolutionDir { get; set; }
        public string ProjectDirectory { get; set; }
        public string IntermediateDir { get; set; }

        public string AssemblyPath { get; set; }

        public string[] References { get; set; }
        public string[] ReferenceCopyLocalPaths { get; set; }

        public string KeyFilePath { get; set; }
        public bool SignAssembly { get; set; }

        public string[] DefineConstants { get; set; }
    }
}