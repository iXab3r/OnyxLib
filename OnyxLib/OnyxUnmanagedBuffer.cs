#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace OnyxLib
{
    public class OnyxUnmanagedBuffer
    {
        public readonly int Length = 0;

        public readonly IntPtr Ptr = IntPtr.Zero;

        public OnyxUnmanagedBuffer(byte[] data)
        {
            Ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Ptr, data.Length);
            Length = data.Length;
        }

        ~OnyxUnmanagedBuffer()
        {
            if (Ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Ptr);
            }
        }
    }
}