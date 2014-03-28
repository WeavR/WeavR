using System;
using System.Linq;
using WeavR.Logging;

namespace WeavR.Tasks
{
    public class AppDomainWorker : MarshalByRefObject
    {
        private readonly TaskConfig config;
        private readonly LoggerContext logger;

        public AppDomainWorker(LoggerContext logger, TaskConfig config)
        {
            this.config = config;
            this.logger = logger;
        }

        public bool Execute()
        {
            logger.LogInfo("Doing some task in {0}", AppDomain.CurrentDomain.FriendlyName);

            return true;
        }
    }
}