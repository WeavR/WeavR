using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WeavR.Tasks
{
    public class Weave : Task
    {
        [Required]
        public ITaskItem AssemblyPath { set; get; }

        [Required]
        public string ProjectDirectory { get; set; }

        [Required]
        public string References { get; set; }

        [Required]
        public ITaskItem[] ReferenceCopyLocalPaths { get; set; }

        [Required]
        public string SolutionDir { get; set; }

        public string IntermediateDir { get; set; }
        public string KeyFilePath { get; set; }
        public bool SignAssembly { get; set; }
        public string DefineConstants { get; set; }

        public override bool Execute()
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press [ENTER] to continue.");
                Console.ReadLine();
            }

            return false;
        }
    }
}