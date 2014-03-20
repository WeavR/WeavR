using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using WeavR.Common;

namespace WeavR.Tasks
{
    public class Weave : Task
    {
        [Required]
        public ITaskItem AssemblyPath { set; get; }

        [Required]
        public ITaskItem ProjectDirectory { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public ITaskItem[] ReferenceCopyLocalPaths { get; set; }

        [Required]
        public ITaskItem SolutionDir { get; set; }

        public ITaskItem IntermediateDir { get; set; }
        public ITaskItem KeyFilePath { get; set; }
        public bool SignAssembly { get; set; }
        public string DefineConstants { get; set; }

        public override bool Execute()
        {
            var logger = new BuildLogger(BuildEngine);

            Engine.Process(new LoggerContext(logger, "WeavR"), ProjectDirectory.FullPath(), AssemblyPath.FullPath(), IntermediateDir.FullPath());

            return !logger.HasLoggedError;
        }
    }
}