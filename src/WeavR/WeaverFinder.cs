using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using WeavR.Common;

namespace WeavR
{
    public class WeaverFinder
    {
        private const string WEAVERS_CONFIG_FILENAME = "WeaversConfig.xml";

        private readonly ILoggerContext logger;
        private readonly ProjectDetails project;

        public WeaverFinder(ILoggerContext logger, ProjectDetails project)
        {
            this.logger = logger;
            this.project = project;
        }

        public IEnumerable<string> FindWeaverConfigs()
        {
            var foundConfigs = new List<string>();

            var solutionConfigFilePath = Path.Combine(project.SolutionDir, WEAVERS_CONFIG_FILENAME);
            if (File.Exists(solutionConfigFilePath))
            {
                foundConfigs.Add(solutionConfigFilePath);
                logger.LogInfo("Found path to weavers file '{0}'.", solutionConfigFilePath);
            }

            var projectConfigFilePath = Path.Combine(project.ProjectDirectory, WEAVERS_CONFIG_FILENAME);
            if (File.Exists(projectConfigFilePath))
            {
                foundConfigs.Add(projectConfigFilePath);
                logger.LogInfo("Found path to weavers file '{0}'.", projectConfigFilePath);
            }

            if (foundConfigs.Count == 0)
            {
                var pathsSearched = string.Join("', '", solutionConfigFilePath, projectConfigFilePath);
                logger.LogWarning("Could not find path to weavers file. Searched '{0}'.", pathsSearched);
            }

            return foundConfigs;
        }

        public IEnumerable<string> FindWeavers()
        {
            var directories = new List<string>();
            directories.Add(Path.Combine(project.SolutionDir, "Packages"));
            directories.Add(Path.Combine(project.SolutionDir, "Tools"));
            directories.Add(GetPackagePath(Path.Combine(project.SolutionDir, "nuget.config")));
            directories.Add(GetPackagePath(Path.Combine(project.SolutionDir, ".nuget", "nuget.config")));
            return directories
                .Where(s => s != null)
                .WhereWithActions(s => Directory.Exists(s), null, s => logger.LogInfo("Skipped scanning '{0}' for weavers since it doesn't exist.", s))
                .SelectMany(d => Directory.EnumerateFiles(d, "*.WeavR.dll", SearchOption.AllDirectories));
        }

        private static string GetPackagePath(string nugetConfigPath)
        {
            if (File.Exists(nugetConfigPath))
            {
                XDocument xDocument;
                try
                {
                    xDocument = XDocument.Load(nugetConfigPath);
                }
                catch (XmlException)
                {
                    return null;
                }
                var repositoryPath = xDocument.Descendants("repositoryPath")
                    .Select(x => x.Value)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (repositoryPath != null)
                {
                    return Path.Combine(Path.GetDirectoryName(nugetConfigPath), repositoryPath);
                }
                repositoryPath = xDocument.Descendants("add")
                    .Where(x => (string)x.Attribute("key") == "repositoryPath")
                    .Select(x => x.Attribute("value"))
                    .Where(x => x != null)
                    .Select(x => x.Value)
                    .FirstOrDefault();
                if (repositoryPath != null)
                {
                    if (repositoryPath.StartsWith("$\\"))
                    {
                        return repositoryPath.Replace("$", Path.Combine(Path.GetDirectoryName(nugetConfigPath)));
                    }

                    return Path.Combine(Path.GetDirectoryName(nugetConfigPath), repositoryPath);
                }
            }
            return null;
        }
    }
}