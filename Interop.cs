using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace BossTrackerMod
{
    internal static class Interop
    {
        private static readonly Dictionary<string, Assembly> interopMods = new()
        {
            { "RecentItemsDisplay", null}
        };

        internal static void FindInteropMods()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (interopMods.ContainsKey(assembly.GetName().Name))
                {
                    interopMods[assembly.GetName().Name] = assembly;
                }
            }
            BossTrackerMod.Instance.Log($"HasRecentItemsDisplay {HasRecentItemsDisplay()}");
        }
        internal static bool HasRecentItemsDisplay()
        {
            return interopMods["RecentItemsDisplay"] is not null;
        }
    }
}