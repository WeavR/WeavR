using System;
using System.Linq;
using Microsoft.Build.Framework;

namespace WeavR.Tasks
{
    public static class TaskItemExtensions
    {
        public static string FullPath(this ITaskItem item)
        {
            return item.GetMetadata("FullPath");
        }

        public static string Filename(this ITaskItem item)
        {
            return item.GetMetadata("Filename");
        }

        public static string Extension(this ITaskItem item)
        {
            return item.GetMetadata("Extension");
        }
    }
}