#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

using XMLib;

using OnyxLib.Extensions;

#endregion

namespace OnyxLib.Snapshot
{
    public class ProcessModuleSnapshot
    {
        public IntPtr BaseAddress;

        public IntPtr EntryPoint;

        public string FileName;

        public List<FunctionSnapshot> Functions;

        public ProcessSnapshot InWhatProcess = null;

        public int ModuleMemorySize;

        public string ModuleName;

        public event EventHandler<EventArgs<FunctionSnapshot>> FunctionFound = delegate { };

        public object Tag;

        public bool is64bit = false;

        public ProcessModuleSnapshot()
        {
        }

        public ProcessModuleSnapshot(ProcessModule pm, ProcessSnapshot ModuleParent = null)
        {
            ModuleName = pm.ModuleNameTryGet("[Access denied]");
            ModuleMemorySize = pm.ModuleMemorySizeTryGet();
            BaseAddress = pm.BaseAddressTryGet();
            FileName = pm.FileNameTryGet();
            EntryPoint = pm.EntryPointAddressTryGet();
            InWhatProcess = ModuleParent;
        }

        public void RefreshExportTable()
        {
            var ExportedFunctions = new List<FunctionSnapshot>();
            if (Functions != null)
            {
                Functions.RemoveAll(delegate(FunctionSnapshot fs) { return fs.FuncType == FunctionType.Exported; });
            }
            var is64bitDll = false;
            var isManagedDll = false;
            LOADED_IMAGE loadedImage;
            OnyxNative.MapAndLoad(FileName, null, out loadedImage, true, true);

            if (loadedImage.MappedAddress != IntPtr.Zero)
            {
                uint size;
                IMAGE_EXPORT_DIRECTORY ExportDir;
                var pExportDir = OnyxNative.ImageDirectoryEntryToData(loadedImage.MappedAddress, false, OnyxNative.IMAGE_DIRECTORY_ENTRY_EXPORT, out size);

                if (pExportDir == IntPtr.Zero)
                {
                    isManagedDll = true;
                    return; // Managed dll;
                }

                ExportDir = (IMAGE_EXPORT_DIRECTORY)Marshal.PtrToStructure(pExportDir, typeof(IMAGE_EXPORT_DIRECTORY));

                var pFuncNames = OnyxNative.RvaToVa(loadedImage, ExportDir.AddressOfNames);
                var pFuncAdressess = OnyxNative.RvaToVa(loadedImage, ExportDir.AddressOfFunctions);
                var pFuncOrdinals = OnyxNative.RvaToVa(loadedImage, ExportDir.AddressOfNameOrdinals);

                for (uint i = 0; i < ExportDir.NumberOfNames; i++)
                {
                    var funcName =
                        Marshal.PtrToStringAnsi((IntPtr)OnyxNative.RvaToVa(loadedImage, (IntPtr)Marshal.ReadInt32((IntPtr)((pFuncNames.ToInt64() + 4 * i)))));
                    var funcOrdinal = (UInt16)(Marshal.ReadInt16((IntPtr)(pFuncOrdinals.ToInt64() + sizeof(UInt16) * i)));
                    var funcAddress = (IntPtr)(Marshal.ReadInt32((IntPtr)(pFuncAdressess.ToInt64() + sizeof(UInt32) * funcOrdinal)));
                    funcOrdinal += (UInt16)ExportDir.Base;
                    var fs = new FunctionSnapshot(funcName, funcAddress, funcOrdinal, FunctionType.Exported, this);
                    ExportedFunctions.Add(fs);
                    FunctionFound(this,new EventArgs<FunctionSnapshot>(fs));
                }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (Functions == null)
            {
                Functions = new List<FunctionSnapshot>();
            }
            Functions.AddRange(ExportedFunctions);
        }

        public uint GetExportedFunctionsCount()
        {
            LOADED_IMAGE loadedImage;
            OnyxNative.MapAndLoad(FileName, null, out loadedImage, true, true);
            if (loadedImage.MappedAddress != IntPtr.Zero)
            {
                uint size;
                IMAGE_EXPORT_DIRECTORY ExportDir;
                var pExportDir = OnyxNative.ImageDirectoryEntryToData(loadedImage.MappedAddress, false, OnyxNative.IMAGE_DIRECTORY_ENTRY_EXPORT, out size);

                if (pExportDir == IntPtr.Zero)
                {
                    return 0; // Managed dll;
                }

                ExportDir = (IMAGE_EXPORT_DIRECTORY)Marshal.PtrToStructure(pExportDir, typeof(IMAGE_EXPORT_DIRECTORY));
                return ExportDir.NumberOfNames;
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", (is64bit) ? "64" : "32", ModuleName);
        }
    }
}