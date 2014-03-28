using System;
using System.Linq;
using WeavR.Logging;

namespace WeavR.Tasks
{
    public class AppDomainWorker : MarshalByRefObject
    {
        private readonly TaskConfig config;
        private readonly Logger logger;

        public AppDomainWorker(Logger logger, TaskConfig config)
        {
            this.config = config;
            this.logger = logger;
        }

        public bool Execute()
        {
            logger.LogInfo("Doing some task in " + AppDomain.CurrentDomain.FriendlyName, "", "WeavR", MessageImportance.High);

            return true;
        }
    }
}