using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeavR.Logging;

namespace WeavR.Tasks
{
    public class ChangeTracker
    {
        private readonly Dictionary<string, DateTime> weaverLastChangeCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly WeaverFinder weaverFinder;

        public ChangeTracker(LoggerContext logger, ProjectDetails projectDetails)
        {
            weaverFinder = new WeaverFinder(logger, projectDetails);

            foreach (var file in weaverFinder.FindWeaverConfigs().Concat(weaverFinder.FindWeavers()))
            {
                weaverLastChangeCache.Add(file, File.GetLastWriteTimeUtc(file));
            }
        }

        public bool FilesChanged()
        {
            return weaverFinder.FindWeaverConfigs().Concat(weaverFinder.FindWeavers())
                .Any(FileChanged);
        }

        private bool FileChanged(string file)
        {
            DateTime lastChanged;
            if (weaverLastChangeCache.TryGetValue(file, out lastChanged))
            {
                // In cache - check time is same
                return lastChanged != File.GetLastWriteTimeUtc(file);
            }
            else
            {
                // Not in cache - check file exists
                return File.Exists(file);
            }
        }
    }
}