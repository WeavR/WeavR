using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;

namespace WeavR.Tasks
{
    public static class TaskItemExtensions
    {
        [DebuggerStepThrough]
        public static string FullPath(this ITaskItem item)
        {
            if (item == null) return "";
            return item.GetMetadata("FullPath");
        }

        [DebuggerStepThrough]
        public static string Filename(this ITaskItem item)
        {
            if (item == null) return "";
            return item.GetMetadata("Filename");
        }

        [DebuggerStepThrough]
        public static string Extension(this ITaskItem item)
        {
            if (item == null) return "";
            return item.GetMetadata("Extension");
        }
    }
}