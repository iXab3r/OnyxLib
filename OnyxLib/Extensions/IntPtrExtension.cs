#region Usings

using System;

#endregion

namespace OnyxLib.Extensions
{
    public static class IntPtrExtension
    {
        public static IntPtr Add(this IntPtr _thisPtr, IntPtr _addPtr)
        {
            if (IntPtr.Size == 4) //32-bit
            {
                return (IntPtr)(_thisPtr.ToInt32() + _addPtr.ToInt32());
            } else
            {
                return (IntPtr)(_thisPtr.ToInt64() + _addPtr.ToInt64());
            }
        }

        public static IntPtr Add(this IntPtr _thisPtr, UInt32 _addPtr)
        {
            if (IntPtr.Size == 4) //32-bit
            {
                return (IntPtr)(_thisPtr.ToInt32() + _addPtr);
            } else
            {
                return (IntPtr)(_thisPtr.ToInt64() + _addPtr);
            }
        }
    }
}