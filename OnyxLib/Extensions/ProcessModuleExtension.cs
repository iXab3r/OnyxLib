#region Usings

using System;
using System.Diagnostics;

#endregion

namespace OnyxLib.Extensions
{
    public static class ProcessModuleExtensions
    {
        public static string ModuleNameTryGet(this ProcessModule pm, string exceptionMessage = null)
        {
            try
            {
                return pm.ModuleName;
            }
            catch (Exception)
            {
                if (exceptionMessage != null)
                {
                    return exceptionMessage;
                } else
                {
                    return null;
                }
            }
        }

        public static string FileNameTryGet(this ProcessModule pm, string exceptionMessage = null)
        {
            try
            {
                return pm.FileName;
            }
            catch (Exception)
            {
                if (exceptionMessage != null)
                {
                    return exceptionMessage;
                } else
                {
                    return null;
                }
            }
        }

        public static int ModuleMemorySizeTryGet(this ProcessModule pm)
        {
            try
            {
                return pm.ModuleMemorySize;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static IntPtr BaseAddressTryGet(this ProcessModule pm)
        {
            try
            {
                return pm.BaseAddress;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public static IntPtr EntryPointAddressTryGet(this ProcessModule pm)
        {
            try
            {
                return pm.EntryPointAddress;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }
    }
}