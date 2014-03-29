using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using WeavR.Logging;

namespace WeavR.Tasks
{
    public class Weave : Task
    {
        private readonly static Dictionary<string, AppDomain> solutionDomains = new Dictionary<string, AppDomain>(StringComparer.OrdinalIgnoreCase);

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
            var rawLogger = new BuildLogger(BuildEngine);

            var logger = new LoggerContext(rawLogger, "WeavR");

            try
            {
                AppDomain context;
                if (solutionDomains.TryGetValue(SolutionDir.FullPath(), out context))
                {
                    if (ChangeAppDomain())
                    {
                        logger.LogInfo("Recreating AppDomain as weavers have changed.");
                        AppDomain.Unload(context);
                        context = solutionDomains[SolutionDir.FullPath()] = CreateDomain();
                    }
                }
                else
                {
                    logger.LogInfo("Creating AppDomain for solution.");
                    context = solutionDomains[SolutionDir.FullPath()] = CreateDomain();
                }

                var remoteTask = CreateProxy<AppDomainWorker>(context, logger, CreateConfig());

                return remoteTask.Execute() && !logger.HasLoggedError;
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                return false;
            }
        }

        private bool ChangeAppDomain()
        {
            // TODO Logic to decide to dump the appdomain and go again
            return false;
        }

        private AppDomain CreateDomain()
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location)
            };
            return AppDomain.CreateDomain("WeavR domain for " + SolutionDir.FullPath(), null, appDomainSetup);
        }

        private T CreateProxy<T>(AppDomain domain, params object[] constructorArgs)
        {
            var type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                type.FullName,
                false,
                System.Reflection.BindingFlags.CreateInstance,
                null,
                constructorArgs,
                null,
                null);
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