#region Usings

using System;

#endregion

namespace OnyxLib.Snapshot
{
    public enum FunctionType
    {
        Exported = 1,

        Custom = 2
    }

    public class FunctionSnapshot
    {
        public readonly FunctionType FuncType;

        public readonly string FunctionName;

        public readonly ProcessModuleSnapshot InWhatModule = null;

        public readonly UInt16 Ordinal;

        public readonly IntPtr RelativeAddress;

        public FunctionSnapshot(
            string szFunctionName,
            IntPtr ptrAddress,
            UInt16 wOrdinal = 0,
            FunctionType ftFunctionType = FunctionType.Custom,
            ProcessModuleSnapshot FunctionParent = null)
        {
            RelativeAddress = ptrAddress;
            FunctionName = szFunctionName;
            InWhatModule = FunctionParent;
            FuncType = ftFunctionType;
        }

        public IntPtr Address
        {
            get
            {
                if (InWhatModule == null)
                {
                    return IntPtr.Zero;
                }
                return (IntPtr)((ulong)RelativeAddress + (ulong)InWhatModule.BaseAddress);
            }
        }

        public override string ToString()
        {
            return String.Format("{2} 0x{0:X8} {1}", RelativeAddress.ToInt64(), FunctionName, (FuncType == FunctionType.Exported) ? "E" : "C");
        }
    }
}