#region Usings

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

#endregion

namespace OnyxLib.MemoryOperations
{
    public class OnyxInjection : MemoryOperationReversable
    {
        private readonly IntPtr _hProcess;

        private readonly IntPtr _injectionAddress;

        private readonly byte[] _injectionBytes;

        /// <summary>
        ///     Initializes a new instance of the OnyxPatch class.
        /// </summary>
        /// <param name="injectionAddress">Address of injection</param>
        /// <param name="injectionBytes">Bytes, that contains injection</param>
        /// <param name="injectionName">Patch name, that will be used in PatchManager</param>
        public OnyxInjection(IntPtr hProcess, byte[] injectionBytes, string injectionName = null)
        {
            if (injectionName != null)
            {
                m_name = injectionName;
            }
            if (hProcess == IntPtr.Zero)
            {
                throw new ArgumentNullException("hProcess");
            }
            _injectionBytes = injectionBytes;
            _hProcess = hProcess;
            _injectionAddress = OnyxMemory.AllocateMemory(_hProcess, (uint)injectionBytes.Length);
        }

        /// <summary>
        ///     Frees allocated memory block and makes calling injected code impossible
        /// </summary>
        /// <returns>True if injection was unapplied</returns>
        public override bool Remove()
        {
            if (OnyxMemory.FreeMemory(_hProcess, _injectionAddress))
            {
                IsApplied = false;
            }
            return IsApplied;
        }

        /// <summary>
        ///     Applies injection
        /// </summary>
        /// <returns>True if injection was succcessfull</returns>
        public override bool Apply()
        {
            if (OnyxMemory.WriteBytes(_hProcess, _injectionAddress, _injectionBytes))
            {
                IsApplied = true;
            }
            return IsApplied;
        }

        /// <summary>
        ///     Executes applied injection, CURRENTLY DOES NOT PROVIDE ANY THREAD ERROR HANDLING
        /// </summary>
        public void Execute(uint waitMilliseconds = 5000)
        {
            if (IsApplied)
            {
                IntPtr threadId, hThread;
                hThread = OnyxNative.CreateRemoteThread(
                    _hProcess,
                    IntPtr.Zero,
                    0,
                    _injectionAddress,
                    IntPtr.Zero,
                    ThreadFlags.THREAD_EXECUTE_IMMEDIATELY,
                    out threadId);
                if (hThread == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (OnyxNative.WaitForSingleObject(hThread, waitMilliseconds) != WaitValues.WAIT_OBJECT_0)
                {
                    throw new TimeoutException("Injected thread timeout");
                }
            } else
            {
                throw new Exception("Injection must first be applied by Apply() method, before you can execute it");
            }
        }
    }
}