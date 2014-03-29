using System;
using System.IO;
using System.Linq;
using WeavR.Common;

namespace WeavR
{
    [Serializable]
    public class ProjectDetails
    {
        public string SolutionDir { get; set; }
        public string ProjectDirectory { get; set; }
        public string AssemblyPath { get; set; }

        public string[] References { get; set; }
        public string[] ReferenceCopyLocalPaths { get; set; }

        public string KeyFilePath { get; set; }
        public bool SignAssembly { get; set; }

        public string[] DefineConstants { get; set; }

        public bool Verify(ILoggerContext logger)
        {
            var result = true;

            if (!File.Exists(AssemblyPath))
            {
                logger.LogError("Assembly '{0}' not found.", AssemblyPath);
                result = false;
            }

            if (!Directory.Exists(ProjectDirectory))
            {
                logger.LogError("Project directory '{0}' not found.", ProjectDirectory);
                result = false;
            }

            return result;
        }
    }
}