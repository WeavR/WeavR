using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WeavR.Tasks
{
    public class RemoteWorker : MarshalByRefObject
    {
        //private readonly WeavR.Common.Logger logger;

        public RemoteWorker()//WeavR.Common.Logger logger)
        {
            //this.logger = logger;
        }

        public bool Execute()
        {
            //logger.LogInfo("Doing some task in " + AppDomain.CurrentDomain.FriendlyName, "", "WeavR", Common.MessageImportance.High);

            return true;
        }
    }

    public class Weave : Task
    {
        private readonly static Dictionary<string, AppDomainContext> solutionDomains = new Dictionary<string, AppDomainContext>(StringComparer.OrdinalIgnoreCase);

        private static readonly Random rng = new Random();

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
                    context = solutionDomains[SolutionDir.FullPath()] = AppDomainContext.Create();
                }
            }
            else
            {
                context = solutionDomains[SolutionDir.FullPath()] = AppDomainContext.Create();
            }

            var temp = Environment.CurrentDirectory;

            var remoteTask = Remote<RemoteWorker>.CreateProxy(context.Domain);

            return remoteTask.RemoteObject.Execute() && !logger.HasLoggedError;
        }

        private bool ChangeAppDomain()
        {
            var dump = rng.Next() % 2 == 0;

            if (dump)
            {
                this.BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Dumping appdomain", "", "WeavR", MessageImportance.High));
            }

            return dump;
        }
    }
}