using System;
using System.Linq;
using WeavR.Logging;

namespace WeavR.Tasks
{
    public class AppDomainWorker : MarshalByRefObject
    {
        private readonly ProjectDetails config;
        private readonly LoggerContext logger;
        private readonly string tempDirectory;

        public AppDomainWorker(LoggerContext logger, ProjectDetails config, string tempDirectory)
        {
            this.config = config;
            this.logger = logger;
            this.tempDirectory = tempDirectory;
        }

        public bool Execute()
        {
            Engine.Process(logger, config, tempDirectory);

            return !logger.HasLoggedError;
        }
    }
}