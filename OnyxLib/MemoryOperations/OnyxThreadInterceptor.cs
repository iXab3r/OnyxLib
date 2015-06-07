#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace OnyxLib.MemoryOperations
{
    internal class OnyxThreadInterceptor : MemoryOperationReversable
    {
        private DlgtCreateThread _dCreateThreadHandler;

        private OnyxDetour _detourCreateThread;

        public OnyxThreadInterceptor()
        {
            var hKernel32 = OnyxNative.LoadLibrary("kernel32.dll");
            var pCreateThread = OnyxNative.GetProcAddress(hKernel32, "CreateThread");

            _dCreateThreadHandler = CreateThreadHandler;
            _detourCreateThread = new OnyxDetour(pCreateThread, _dCreateThreadHandler);
            _detourCreateThread.IgnoreThreadChecking = true;
        }

        private IntPtr CreateThreadHandler(
            IntPtr lpSecurityAttributes,
            uint StackSize,
            IntPtr lpStartFunction,
            IntPtr lpThreadParameter,
            uint CreationFlags,
            IntPtr lpThreadId)
        {
            var hThread =
                (IntPtr)_detourCreateThread.CallOriginal(lpSecurityAttributes, StackSize, lpStartFunction, lpThreadParameter, CreationFlags, lpThreadId);
            var threadId = OnyxNative.GetThreadId(hThread);
            var currentThreadId = OnyxNative.GetCurrentThreadId();

            var addToExcluded = true;
            addToExcluded = addToExcluded & Onyx.Instance.Detours.GlobalExcludedThreadId.Contains(currentThreadId);
            if (addToExcluded)
            {
                Onyx.Instance.Detours.GlobalExcludedThreadId.Add(threadId);
            }

            /*string temp = String.Empty;
            foreach (var i in Onyx.Instance.Detours.GlobalExcludedThreadId)
            {
                temp += " " + i.ToString("X4");
            }
            System.Windows.Forms.MessageBox.Show(String.Format("Thread {0:X4} created by thread {1:X4} \r\n Excluded: {2}", threadId, currentThreadId,temp));*/
            return hThread;
        }

        public override bool Remove()
        {
            return _detourCreateThread.Remove();
        }

        public override bool Apply()
        {
            return _detourCreateThread.Apply();
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr DlgtCreateThread(
            IntPtr lpSecurityAttributes,
            uint StackSize,
            IntPtr lpStartFunction,
            IntPtr lpThreadParameter,
            uint CreationFlags,
            IntPtr lpThreadId);
    }
}