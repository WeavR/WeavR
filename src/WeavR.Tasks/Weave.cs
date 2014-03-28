using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppDomainToolkit;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WeavR.Tasks
{
    public class Weave : Task
    {
        private readonly static Dictionary<string, AppDomainContext> solutionDomains = new Dictionary<string, AppDomainContext>(StringComparer.OrdinalIgnoreCase);

        [Required]
        public ITaskItem SolutionDir { get; set; }

        [Required]
        public ITaskItem AssemblyPath { set; get; }

        [Required]
        public ITaskItem ProjectDirectory { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public ITaskItem[] ReferenceCopyLocalPaths { get; set; }

        public ITaskItem IntermediateDir { get; set; }
        public ITaskItem KeyFilePath { get; set; }
        public bool SignAssembly { get; set; }
        public string DefineConstants { get; set; }

        public override bool Execute()
        {
            var logger = new BuildLogger(BuildEngine);

            AppDomainContext context;
            if (solutionDomains.TryGetValue(SolutionDir.FullPath(), out context))
            {
                if (ChangeAppDomain())
                {
                    context.Dispose();
                    context = solutionDomains[SolutionDir.FullPath()] = CreateDomain();
                }
            }
            else
            {
                context = solutionDomains[SolutionDir.FullPath()] = CreateDomain();
            }

            var remoteTask = Remote<AppDomainWorker>.CreateProxy(context.Domain, logger, CreateConfig());

            return remoteTask.RemoteObject.Execute() && !logger.HasLoggedError;
        }

        private bool ChangeAppDomain()
        {
            // TODO Logic to decide to dump the appdomain and go again
            return false;
        }

        private AppDomainContext CreateDomain()
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location)
            };
            return AppDomainContext.Create(appDomainSetup);
        }

        private TaskConfig CreateConfig()
        {
            return new TaskConfig
            {
                SolutionDir = SolutionDir.FullPath(),
                ProjectDirectory = ProjectDirectory.FullPath(),
                IntermediateDir = IntermediateDir.FullPath(),

                AssemblyPath = AssemblyPath.FullPath(),

                References = References.IgnoreNull().Select(r => r.FullPath()).ToArray(),
                ReferenceCopyLocalPaths = ReferenceCopyLocalPaths.IgnoreNull().Select(r => r.FullPath()).ToArray(),

                KeyFilePath = KeyFilePath.FullPath(),
                SignAssembly = SignAssembly,

                DefineConstants = DefineConstants.Split(';')
            };
        }
    }
}