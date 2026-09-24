using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Developer.Console;

namespace SHUU.UserSide.Addons.CullingSystem
{
    public class CullingSystem_DevConsoleCommands : MonoBehaviour
    {
        #region Variables
        private static CommandReturn NoInstance() => CommandReturn.Red("There's no SHUU_Culling in the scene.");
        #endregion




        #region Commands
        [DevConsoleCommand("cullingstats", "Shows how many objects are registered with the SHUU_Culling and how many are currently culled.", "Information")]
        public static CommandReturn CullingStats()
        {
            SHUU_Culling manager = SHUU_Culling.Instance;
            if (manager == null) return NoInstance();

            return new CommandReturn($"{manager.CulledCount}/{manager.RegisteredCount} culled.");
        }


        [DevConsoleCommand("cullinglist", "Lists every object registered with the SHUU_Culling and whether it's currently culled.", "Information")]
        public static CommandReturn CullingList()
        {
            SHUU_Culling manager = SHUU_Culling.Instance;
            if (manager == null) return NoInstance();

            List<string> lines = new List<string>();
            foreach (ICullable cullable in manager.Cullables)
            {
                string name = cullable.CullTransform != null ? cullable.CullTransform.name : "(no transform)";
                lines.Add($"{name}: {(cullable.IsCulled ? "culled" : "visible")}");
            }

            if (lines.Count == 0) return CommandReturn.Yellow("Nothing is registered with the SHUU_Culling.");

            return new CommandReturn(lines.ToArray());
        }


        [DevConsoleCommand("cullingforce", "Forces an immediate re-check of every registered object instead of waiting for the stagger.", "Debug")]
        public static CommandReturn CullingForce()
        {
            SHUU_Culling manager = SHUU_Culling.Instance;
            if (manager == null) return NoInstance();

            manager.ForceReevaluate();

            return CommandReturn.Green("Re-checked every registered object.");
        }
        #endregion
    }
}
