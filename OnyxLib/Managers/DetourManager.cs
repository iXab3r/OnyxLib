#region Usings

using System;
using System.Collections.Generic;

using OnyxLib.MemoryOperations;

#endregion

namespace OnyxLib.Managers
{
    public class DetourManager : OnyxManager<OnyxDetour>
    {
        /// <summary>
        ///     Global set of excluded thread Ids, all calls from these threads will not be hooked by any detour
        /// </summary>
        public HashSet<uint> GlobalExcludedThreadId = new HashSet<uint>();

        /// <summary>
        ///     Creates a new Detour.
        /// </summary>
        /// <param name="targetAddress">The original function to detour.</param>
        /// <param name="detour">The new function to be called. (This delegate should NOT be registered!)</param>
        /// <param name="name">The name of the detour.</param>
        /// <returns>
        ///     A <see cref="Detour" /> object containing the required methods to apply, remove, and call the original
        ///     function.
        /// </returns>
        public OnyxDetour Create(IntPtr targetAddress, Delegate detour, string name)
        {
            if (name != "" && Operations.ContainsKey(name))
            {
                throw new ArgumentException(string.Format("The {0} detour already exists!", name), "name");
            }

            var d = new OnyxDetour(targetAddress, detour, name);
            if (name != "")
            {
                Operations.Add(name, d);
            }
            return d;
        }

        /// <summary>
        ///     Creates and applies new Detour.
        /// </summary>
        /// <param name="targetAddress">The original function to detour.</param>
        /// <param name="detour">The new function to be called. (This delegate should NOT be registered!)</param>
        /// <param name="name">The name of the detour.</param>
        /// <returns>Result of injecting</returns>
        public bool CreateAndApply(IntPtr targetAddress, Delegate detour, string name)
        {
            var ret = Create(targetAddress, detour, name);
            return ret.Apply();
        }
    }
}