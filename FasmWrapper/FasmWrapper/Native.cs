#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FasmWrapper
{
    internal static class Native
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string _lpFileName);
    }
}