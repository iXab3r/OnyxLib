#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

#endregion

namespace OnyxLib
{

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;

        public IntPtr lpSecurityDescriptor;

        public int bInheritHandle;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_IMPORT_BY_NAME
    {
        [FieldOffset(0)]
        public ushort Hint;

        [FieldOffset(2)]
        public char Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_NT_HEADERS64
    {
        /// DWORD->unsigned int
        public uint Signature;

        /// IMAGE_FILE_HEADER->_IMAGE_FILE_HEADER
        public IMAGE_FILE_HEADER FileHeader;

        /// IMAGE_OPTIONAL_HEADER64->_IMAGE_OPTIONAL_HEADER64
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _MODULEINFO
    {
        public IntPtr BaseOfDll;

        public uint SizeOfImage;

        public IntPtr EntryPoint;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_FILE_HEADER
    {
        /// WORD->unsigned short
        public ushort Machine;

        /// WORD->unsigned short
        public ushort NumberOfSections;

        /// DWORD->unsigned int
        public uint TimeDateStamp;

        /// DWORD->unsigned int
        public uint PointerToSymbolTable;

        /// DWORD->unsigned int
        public uint NumberOfSymbols;

        /// WORD->unsigned short
        public ushort SizeOfOptionalHeader;

        /// WORD->unsigned short
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_OPTIONAL_HEADER64
    {
        /// WORD->unsigned short
        public ushort Magic;

        /// BYTE->unsigned char
        public byte MajorLinkerVersion;

        /// BYTE->unsigned char
        public byte MinorLinkerVersion;

        /// DWORD->unsigned int
        public uint SizeOfCode;

        /// DWORD->unsigned int
        public uint SizeOfInitializedData;

        /// DWORD->unsigned int
        public uint SizeOfUninitializedData;

        /// DWORD->unsigned int
        public uint AddressOfEntryPoint;

        /// DWORD->unsigned int
        public uint BaseOfCode;

        /// ULONGLONG->unsigned __int64
        public ulong ImageBase;

        /// DWORD->unsigned int
        public uint SectionAlignment;

        /// DWORD->unsigned int
        public uint FileAlignment;

        /// WORD->unsigned short
        public ushort MajorOperatingSystemVersion;

        /// WORD->unsigned short
        public ushort MinorOperatingSystemVersion;

        /// WORD->unsigned short
        public ushort MajorImageVersion;

        /// WORD->unsigned short
        public ushort MinorImageVersion;

        /// WORD->unsigned short
        public ushort MajorSubsystemVersion;

        /// WORD->unsigned short
        public ushort MinorSubsystemVersion;

        /// DWORD->unsigned int
        public uint Win32VersionValue;

        /// DWORD->unsigned int
        public uint SizeOfImage;

        /// DWORD->unsigned int
        public uint SizeOfHeaders;

        /// DWORD->unsigned int
        public uint CheckSum;

        /// WORD->unsigned short
        public ushort Subsystem;

        /// WORD->unsigned short
        public ushort DllCharacteristics;

        /// ULONGLONG->unsigned __int64
        public ulong SizeOfStackReserve;

        /// ULONGLONG->unsigned __int64
        public ulong SizeOfStackCommit;

        /// ULONGLONG->unsigned __int64
        public ulong SizeOfHeapReserve;

        /// ULONGLONG->unsigned __int64
        public ulong SizeOfHeapCommit;

        /// DWORD->unsigned int
        public uint LoaderFlags;

        /// DWORD->unsigned int
        public uint NumberOfRvaAndSizes;

        /// IMAGE_DATA_DIRECTORY[16]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.Struct)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        /// DWORD->unsigned int
        public uint VirtualAddress;

        /// DWORD->unsigned int
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_EXPORT_DIRECTORY
    {
        /// DWORD->unsigned int
        public uint Characteristics;

        /// DWORD->unsigned int
        public uint TimeDateStamp;

        /// WORD->unsigned short
        public ushort MajorVersion;

        /// WORD->unsigned short
        public ushort MinorVersion;

        /// DWORD->unsigned int
        public uint Name;

        /// DWORD->unsigned int
        public uint Base;

        /// DWORD->unsigned int
        public uint NumberOfFunctions;

        /// DWORD->unsigned int
        public uint NumberOfNames;

        /// DWORD->unsigned int
        public uint AddressOfFunctions;

        /// DWORD->unsigned int
        public uint AddressOfNames;

        /// DWORD->unsigned int
        public uint AddressOfNameOrdinals;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_NT_HEADERS32
    {
        /// DWORD->unsigned int
        public uint Signature;

        /// IMAGE_FILE_HEADER->_IMAGE_FILE_HEADER
        public IMAGE_FILE_HEADER FileHeader;

        /// IMAGE_OPTIONAL_HEADER32->_IMAGE_OPTIONAL_HEADER
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_OPTIONAL_HEADER32
    {
        /// WORD->unsigned short
        public ushort Magic;

        /// BYTE->unsigned char
        public byte MajorLinkerVersion;

        /// BYTE->unsigned char
        public byte MinorLinkerVersion;

        /// DWORD->unsigned int
        public uint SizeOfCode;

        /// DWORD->unsigned int
        public uint SizeOfInitializedData;

        /// DWORD->unsigned int
        public uint SizeOfUninitializedData;

        /// DWORD->unsigned int
        public uint AddressOfEntryPoint;

        /// DWORD->unsigned int
        public uint BaseOfCode;

        /// DWORD->unsigned int
        public uint BaseOfData;

        /// DWORD->unsigned int
        public uint ImageBase;

        /// DWORD->unsigned int
        public uint SectionAlignment;

        /// DWORD->unsigned int
        public uint FileAlignment;

        /// WORD->unsigned short
        public ushort MajorOperatingSystemVersion;

        /// WORD->unsigned short
        public ushort MinorOperatingSystemVersion;

        /// WORD->unsigned short
        public ushort MajorImageVersion;

        /// WORD->unsigned short
        public ushort MinorImageVersion;

        /// WORD->unsigned short
        public ushort MajorSubsystemVersion;

        /// WORD->unsigned short
        public ushort MinorSubsystemVersion;

        /// DWORD->unsigned int
        public uint Win32VersionValue;

        /// DWORD->unsigned int
        public uint SizeOfImage;

        /// DWORD->unsigned int
        public uint SizeOfHeaders;

        /// DWORD->unsigned int
        public uint CheckSum;

        /// WORD->unsigned short
        public ushort Subsystem;

        /// WORD->unsigned short
        public ushort DllCharacteristics;

        /// DWORD->unsigned int
        public uint SizeOfStackReserve;

        /// DWORD->unsigned int
        public uint SizeOfStackCommit;

        /// DWORD->unsigned int
        public uint SizeOfHeapReserve;

        /// DWORD->unsigned int
        public uint SizeOfHeapCommit;

        /// DWORD->unsigned int
        public uint LoaderFlags;

        /// DWORD->unsigned int
        public uint NumberOfRvaAndSizes;

        /// IMAGE_DATA_DIRECTORY[16]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.Struct)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DOS_HEADER
    {
        public UInt16 e_magic; // Magic number

        public UInt16 e_cblp; // Bytes on last page of file

        public UInt16 e_cp; // Pages in file

        public UInt16 e_crlc; // Relocations

        public UInt16 e_cparhdr; // Size of header in paragraphs

        public UInt16 e_minalloc; // Minimum extra paragraphs needed

        public UInt16 e_maxalloc; // Maximum extra paragraphs needed

        public UInt16 e_ss; // Initial (relative) SS value

        public UInt16 e_sp; // Initial SP value

        public UInt16 e_csum; // Checksum

        public UInt16 e_ip; // Initial IP value

        public UInt16 e_cs; // Initial (relative) CS value

        public UInt16 e_lfarlc; // File address of relocation table

        public UInt16 e_ovno; // Overlay number

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt16[] e_res1; // Reserved words

        public UInt16 e_oemid; // OEM identifier (for e_oeminfo)

        public UInt16 e_oeminfo; // OEM information; e_oemid specific

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt16[] e_res2; // Reserved words

        public Int32 e_lfanew; // File address of new exe header
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_IMPORT_DESCRIPTOR
    {
        #region union

        /// <summary>
        ///     CSharp doesnt really support unions, but they can be emulated by a field offset 0
        /// </summary>
        [FieldOffset(0)]
        public uint Characteristics; // 0 for terminating null import descriptor

        [FieldOffset(0)]
        public uint OriginalFirstThunk; // RVA to original unbound IAT (PIMAGE_THUNK_DATA)

        #endregion

        [FieldOffset(4)]
        public uint TimeDateStamp;

        [FieldOffset(8)]
        public uint ForwarderChain;

        [FieldOffset(12)]
        public uint Name;

        [FieldOffset(16)]
        public uint FirstThunk;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct THUNK_DATA
    {
        [FieldOffset(0)]
        public uint ForwarderString; // PBYTE 

        [FieldOffset(4)]
        public uint Function; // PDWORD

        [FieldOffset(8)]
        public uint Ordinal;

        [FieldOffset(12)]
        public uint AddressOfData; // PIMAGE_IMPORT_BY_NAME
    }

    // ReSharper disable InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    public struct LOADED_IMAGE
    {
        public IntPtr moduleName;

        public IntPtr hFile;

        public IntPtr MappedAddress;

        public IntPtr FileHeader;

        public IntPtr lastRvaSection;

        public UInt32 numbOfSections;

        public IntPtr firstRvaSection;

        public UInt32 charachteristics;

        public ushort systemImage;

        public ushort dosImage;

        public ushort readOnly;

        public ushort version;

        public IntPtr links_1; // these two comprise the LIST_ENTRY

        public IntPtr links_2;

        public UInt32 sizeOfImage;
    }

    /// <summary>
    ///     Values which determine the state or creation-state of a thread.
    /// </summary>
    [Flags]
    public enum ThreadFlags : uint
    {
        /// <summary>
        ///     The thread will execute immediately.
        /// </summary>
        CREATE_SUSPENDED = 0x04,

        /// <summary>
        ///     The thread will be created in a suspended state.  Use <see cref="Imports.ResumeThread" /> to resume the thread.
        /// </summary>
        STACK_SIZE_PARAM_IS_A_RESERVATION = 0x00010000,

        /// <summary>
        ///     The dwStackSize parameter specifies the initial reserve size of the stack. If this flag is not specified,
        ///     dwStackSize specifies the commit size.
        /// </summary>
        STILL_ACTIVE = 259,

        /// <summary>
        ///     The thread is still active.
        /// </summary>
        THREAD_EXECUTE_IMMEDIATELY = 0
    }

    [Flags]
    public enum WaitValues : uint
    {
        /// <summary>
        ///     The object is in a signaled state.
        /// </summary>
        INFINITE = 0xFFFFFFFF,

        /// <summary>
        ///     The specified object is a mutex object that was not released by the thread that owned the mutex object before the
        ///     owning thread terminated. Ownership of the mutex object is granted to the calling thread, and the mutex is set to
        ///     nonsignaled.
        /// </summary>
        WAIT_ABANDONED = 0x00000080,

        /// <summary>
        ///     The time-out interval elapsed, and the object's state is nonsignaled.
        /// </summary>
        WAIT_FAILED = 0xFFFFFFFF,

        /// <summary>
        ///     The wait has failed.
        /// </summary>
        WAIT_OBJECT_0 = 0x00000000,

        /// <summary>
        ///     Wait an infinite amount of time for the object to become signaled.
        /// </summary>
        WAIT_TIMEOUT = 0x00000102
    }

    [Flags]
    public enum EFileAccess : uint
    {
        Delete = 0x10000,

        ReadControl = 0x20000,

        WriteDAC = 0x40000,

        WriteOwner = 0x80000,

        Synchronize = 0x100000,

        StandardRightsRequired = 0xF0000,

        StandardRightsRead = ReadControl,

        StandardRightsWrite = ReadControl,

        StandardRightsExecute = ReadControl,

        StandardRightsAll = 0x1F0000,

        SpecificRightsAll = 0xFFFF,

        AccessSystemSecurity = 0x1000000, // AccessSystemAcl access type

        MaximumAllowed = 0x2000000, // MaximumAllowed access type

        GenericRead = 0x80000000,

        GenericWrite = 0x40000000,

        GenericExecute = 0x20000000,

        GenericAll = 0x10000000
    }

    [Flags]
    public enum EFileShare : uint
    {
        /// <summary>
        /// </summary>
        None = 0x00000000,

        /// <summary>
        ///     Enables subsequent open operations on an object to request read access.
        ///     Otherwise, other processes cannot open the object if they request read access.
        ///     If this flag is not specified, but the object has been opened for read access, the function fails.
        /// </summary>
        Read = 0x00000001,

        /// <summary>
        ///     Enables subsequent open operations on an object to request write access.
        ///     Otherwise, other processes cannot open the object if they request write access.
        ///     If this flag is not specified, but the object has been opened for write access, the function fails.
        /// </summary>
        Write = 0x00000002,

        /// <summary>
        ///     Enables subsequent open operations on an object to request delete access.
        ///     Otherwise, other processes cannot open the object if they request delete access.
        ///     If this flag is not specified, but the object has been opened for delete access, the function fails.
        /// </summary>
        Delete = 0x00000004
    }

    public enum ECreationDisposition : uint
    {
        /// <summary>
        ///     Creates a new file. The function fails if a specified file exists.
        /// </summary>
        New = 1,

        /// <summary>
        ///     Creates a new file, always.
        ///     If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file
        ///     attributes,
        ///     and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES
        ///     structure specifies.
        /// </summary>
        CreateAlways = 2,

        /// <summary>
        ///     Opens a file. The function fails if the file does not exist.
        /// </summary>
        OpenExisting = 3,

        /// <summary>
        ///     Opens a file, always.
        ///     If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OpenAlways = 4,

        /// <summary>
        ///     Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        ///     The calling process must open the file with the GENERIC_WRITE access right.
        /// </summary>
        TruncateExisting = 5
    }

    [Flags]
    public enum EFileAttributes : uint
    {
        Readonly = 0x00000001,

        Hidden = 0x00000002,

        System = 0x00000004,

        Directory = 0x00000010,

        Archive = 0x00000020,

        Device = 0x00000040,

        Normal = 0x00000080,

        Temporary = 0x00000100,

        SparseFile = 0x00000200,

        ReparsePoint = 0x00000400,

        Compressed = 0x00000800,

        Offline = 0x00001000,

        NotContentIndexed = 0x00002000,

        Encrypted = 0x00004000,

        Write_Through = 0x80000000,

        Overlapped = 0x40000000,

        NoBuffering = 0x20000000,

        RandomAccess = 0x10000000,

        SequentialScan = 0x08000000,

        DeleteOnClose = 0x04000000,

        BackupSemantics = 0x02000000,

        PosixSemantics = 0x01000000,

        OpenReparsePoint = 0x00200000,

        OpenNoRecall = 0x00100000,

        FirstPipeInstance = 0x00080000
    }

    #endregion

    public class OnyxNative
    {
        #region DLLImports

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion

        [Flags]
        public enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,

            FileMapWrite = 0x0002,

            FileMapRead = 0x0004,

            FileMapAllAccess = 0x001f,

            FileMapExecute = 0x0020,

            fileMapExecute = 0x0020,
        }

        [Flags]
        public enum FileMapProtection : uint
        {
            PageReadonly = 0x02,

            PageReadWrite = 0x04,

            PageWriteCopy = 0x08,

            PageExecuteRead = 0x20,

            PageExecuteReadWrite = 0x40,

            SectionCommit = 0x8000000,

            SectionImage = 0x1000000,

            SectionNoCache = 0x10000000,

            SectionReserve = 0x4000000,
        }

        public static IntPtr RvaToVa(LOADED_IMAGE loadedImage, uint rva)
        {
            return ImageRvaToVa(loadedImage.FileHeader, loadedImage.MappedAddress, rva, IntPtr.Zero);
        }

        public static IntPtr RvaToVa(LOADED_IMAGE loadedImage, IntPtr rva)
        {
            return RvaToVa(loadedImage, (uint)(rva.ToInt32()));
        }

        public static IntPtr GetModuleHandle(IntPtr hProcess, string moduleName)
        {
            bool isWow64;
            IsWow64Process(hProcess, out isWow64);
            var moduleBitness = (isWow64) ? 1 : 2;
            var lpcbNeeded = 0;
            IntPtr[] hModules = null;
            EnumProcessModulesEx(hProcess, null, 0, out lpcbNeeded, moduleBitness); // Пробуем получить lpcbNeeded
            var arrSize = lpcbNeeded / IntPtr.Size;
            hModules = new IntPtr[arrSize];

            if (EnumProcessModulesEx(hProcess, hModules, lpcbNeeded, out lpcbNeeded, moduleBitness))
            {
                var nameFile = new StringBuilder(1024);
                for (var i = 0; i < arrSize; i++)
                {
                    GetModuleFileNameEx(hProcess, hModules[i], nameFile, nameFile.Capacity);
                    if (String.Compare(Path.GetFileName(nameFile.ToString()), moduleName, true) == 0)
                    {
                        return hModules[i];
                    }
                }
            }
            return IntPtr.Zero;
        }

        public static bool Is64bitProcess(IntPtr hProcess)
        {
            if (hProcess == IntPtr.Zero)
            {
                throw new ArgumentException("Wrong process handle !");
            }
            bool Result;
            if (!IsWow64Process(hProcess, out Result)) // Exception occured
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return !Result;
        }

        public static bool Is64bitProcess(int PID)
        {
            return Is64bitProcess(Process.GetProcessById(PID).Handle);
        }

        public static string GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule)
        {
            var fileName = new StringBuilder(1024);

            if (GetModuleFileNameEx(hProcess, hModule, fileName, fileName.Capacity) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return fileName.ToString();
        }

        public static string GetSystemDirectory()
        {
            var sysdyrPath = new StringBuilder(255);
            GetSystemDirectory(sysdyrPath, (uint)sysdyrPath.MaxCapacity);
            return sysdyrPath.ToString();
        }

        public static string GetSystemWow64Directory()
        {
            var sysdyrPath = new StringBuilder(255);
            GetSystemWow64Directory(sysdyrPath, (uint)sysdyrPath.MaxCapacity);
            return sysdyrPath.ToString();
        }

        public static IntPtr GetExportedFunctionRVA(string dllName, string functionName)
        {
            LOADED_IMAGE loadedImage;
            if (String.IsNullOrEmpty(dllName))
            {
                throw new ArgumentNullException("dllName");
            }
            MapAndLoad(dllName, null, out loadedImage, true, true);
            if (loadedImage.MappedAddress != IntPtr.Zero)
            {
                uint size;
                IMAGE_EXPORT_DIRECTORY ExportDir;
                var pExportDir = ImageDirectoryEntryToData(loadedImage.MappedAddress, false, IMAGE_DIRECTORY_ENTRY_EXPORT, out size);

                if (pExportDir == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                ExportDir = (IMAGE_EXPORT_DIRECTORY)Marshal.PtrToStructure(pExportDir, typeof(IMAGE_EXPORT_DIRECTORY));
                var pFuncNames = RvaToVa(loadedImage, ExportDir.AddressOfNames);
                var pFuncAdressess = RvaToVa(loadedImage, ExportDir.AddressOfFunctions);
                var pFuncOrdinals = RvaToVa(loadedImage, ExportDir.AddressOfNameOrdinals);
                for (uint i = 0; i < ExportDir.NumberOfNames; i++)
                {
                    var funcName = Marshal.PtrToStringAnsi((IntPtr)RvaToVa(loadedImage, (IntPtr)Marshal.ReadInt32((IntPtr)((pFuncNames.ToInt64() + 4 * i)))));
                    var funcOrdinal = (UInt16)(Marshal.ReadInt16((IntPtr)(pFuncOrdinals.ToInt64() + sizeof(UInt16) * i)));
                    var funcAddress = (IntPtr)(Marshal.ReadInt32((IntPtr)(pFuncAdressess.ToInt64() + sizeof(UInt32) * funcOrdinal)));
                    funcOrdinal += (UInt16)ExportDir.Base;
                    if (String.Compare(funcName, functionName, true) == 0)
                    {
                        return funcAddress; //0x0006FD3F
                    }
                }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return IntPtr.Zero; //nothing found
        }

        public static bool IsValidHandle(IntPtr hObject)
        {
            uint dwFlags;
            return (GetHandleInformation(hObject, out dwFlags));
        }

        #region DLL export/import table

        public static string[] GetExportedFunctions(string DllName)
        {
            var FuncList = new List<string>();
            LOADED_IMAGE loadedImage;
            MapAndLoad(DllName, null, out loadedImage, true, true);
            var isManagedDll = false;
            if (loadedImage.MappedAddress != IntPtr.Zero)
            {
                uint size;
                IMAGE_EXPORT_DIRECTORY pExportDir;
                try
                {
                    pExportDir =
                        (IMAGE_EXPORT_DIRECTORY)
                            Marshal.PtrToStructure(
                                ImageDirectoryEntryToData(loadedImage.MappedAddress, false, IMAGE_DIRECTORY_ENTRY_EXPORT, out size),
                                typeof(IMAGE_EXPORT_DIRECTORY));
                }
                catch (NullReferenceException)
                {
                    isManagedDll = true;
                    FuncList.Add("Export table is empty, possibly it is managed dll");
                    return FuncList.ToArray();
                }
                var pFuncNames = RvaToVa(loadedImage, pExportDir.AddressOfNames);
                var pFuncAdressess = RvaToVa(loadedImage, pExportDir.AddressOfFunctions);
                var pFuncOrdinals = RvaToVa(loadedImage, pExportDir.AddressOfNameOrdinals);
                for (uint i = 0; i < pExportDir.NumberOfNames; i++)
                {
                    //uint funcNameRva = pFuncNames[i];                    
                    var szFunctionName =
                        Marshal.PtrToStringAnsi((IntPtr)RvaToVa(loadedImage, (IntPtr)Marshal.ReadInt32((IntPtr)((pFuncNames.ToInt64() + 4 * i)))));
                    var dwAddress =
                        (IntPtr)
                            (RvaToVa(loadedImage, (IntPtr)(Marshal.ReadInt32((IntPtr)(pFuncAdressess.ToInt64() + 4 * i)))).ToInt64()
                             - loadedImage.MappedAddress.ToInt64());
                    FuncList.Add(String.Format("0x{0:X8} {1}", dwAddress.ToInt32(), szFunctionName));
                }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (isManagedDll)
            {
                FuncList.Add("Export table is empty, possibly it is managed dll");
            }
            return FuncList.ToArray();
        }

        private static IntPtr MarshalToPointer(object data)
        {
            var buf = Marshal.AllocHGlobal(Marshal.SizeOf(data));
            Marshal.StructureToPtr(data, buf, false);
            return buf;
        }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "IsBadReadPtr")]
        [SuppressUnmanagedCodeSecurity]
        public static extern unsafe bool IsBadReadPtr(void* lpBase, uint ucb);

        public static unsafe string[] GetImportedFunctions(string DllName)
        {
            var funcList = new List<string>();
            LOADED_IMAGE loadedImage;
            MapAndLoad(DllName, null, out loadedImage, true, true);
            if (loadedImage.MappedAddress != IntPtr.Zero)
            {
                uint size;
                try
                {
                    var test = (IMAGE_IMPORT_DESCRIPTOR*)ImageDirectoryEntryToData(loadedImage.MappedAddress, false, IMAGE_DIRECTORY_ENTRY_IMPORT, out size);
                    if ((IntPtr)test == IntPtr.Zero)
                    {
                        throw new NullReferenceException();
                    }
                }
                catch (NullReferenceException)
                {
                    funcList.Add(String.Format("Empty IMAGE_DIRECTORY_ENTRY_IMPORT data, possible protected image"));
                    return funcList.ToArray();
                }
                var pImportDir = (IMAGE_IMPORT_DESCRIPTOR*)ImageDirectoryEntryToData(loadedImage.MappedAddress, false, IMAGE_DIRECTORY_ENTRY_IMPORT, out size);
                while (pImportDir->OriginalFirstThunk != 0)
                {
                    try
                    {
                        var szName = (char*)RvaToVa(loadedImage, pImportDir->Name);
                        var name = Marshal.PtrToStringAnsi((IntPtr)szName);
                        var pThunkOrg = (THUNK_DATA*)RvaToVa(loadedImage, pImportDir->OriginalFirstThunk);
                        while (pThunkOrg->AddressOfData != 0)
                        {
                            uint ord;
                            if ((pThunkOrg->Ordinal & 0x80000000) > 0)
                            {
                                ord = pThunkOrg->Ordinal & 0xffff;
                                funcList.Add(String.Format("imports ({0}).Ordinal{1} - Address: 0x{2:X8}", name, ord, pThunkOrg->Function));
                            } else
                            {
                                var pImageByName = (IMAGE_IMPORT_BY_NAME*)RvaToVa(loadedImage, pThunkOrg->AddressOfData);
                                if (!IsBadReadPtr(pImageByName, (uint)sizeof(IMAGE_IMPORT_BY_NAME)))
                                {
                                    ord = pImageByName->Hint;
                                    var szImportName = pImageByName->Name;
                                    var sImportName = Marshal.PtrToStringAnsi((IntPtr)szImportName);
                                    funcList.Add(String.Format("imports ({0}).{1}@{2} - Address: 0x{3:X8}", name, sImportName, ord, pThunkOrg->Function));
                                } else
                                {
                                    funcList.Add(String.Format("Bad ReadPtr Detected or EOF on Imports"));
                                    break;
                                }
                            }
                            pThunkOrg++;
                        }
                    }
                    catch (AccessViolationException e)
                    {
                        funcList.Add(String.Format("An Access violation occured\n" + "this seems to suggest the end of the imports section\n"));
                        funcList.Add(String.Format(e.ToString()));
                    }
                    pImportDir++;
                }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return funcList.ToArray();
        }

        #endregion

        /// <summary>
        ///     Values to gain required access to process or thread.
        /// </summary>
        public static class AccessRights
        {
            /// <summary>
            ///     Standard rights required to mess with an object's security descriptor, change, or delete the object.
            /// </summary>
            public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;

            /// <summary>
            ///     The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled
            ///     state. Some object types do not support this access right.
            /// </summary>
            public const uint SYNCHRONIZE = 0x00100000;

            /// <summary>
            ///     Required to terminate a process using TerminateProcess.
            /// </summary>
            public const uint PROCESS_TERMINATE = 0x0001;

            /// <summary>
            ///     Required to create a thread.
            /// </summary>
            public const uint PROCESS_CREATE_THREAD = 0x0002;

            //public const uint PROCESS_SET_SESSIONID = 0x0004;
            /// <summary>
            ///     Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            /// </summary>
            public const uint PROCESS_VM_OPERATION = 0x0008;

            /// <summary>
            ///     Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            public const uint PROCESS_VM_READ = 0x0010;

            /// <summary>
            ///     Required to write memory in a process using WriteProcessMemory.
            /// </summary>
            public const uint PROCESS_VM_WRITE = 0x0020;

            /// <summary>
            ///     Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            public const uint PROCESS_DUP_HANDLE = 0x0040;

            /// <summary>
            ///     Required to create a process.
            /// </summary>
            public const uint PROCESS_CREATE_PROCESS = 0x0080;

            /// <summary>
            ///     Required to set memory limits using SetProcessWorkingSetSize.
            /// </summary>
            public const uint PROCESS_SET_QUOTA = 0x0100;

            /// <summary>
            ///     Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            /// </summary>
            public const uint PROCESS_SET_INFORMATION = 0x0200;

            /// <summary>
            ///     Required to retrieve certain information about a process, such as its token, exit code, and priority class (see
            ///     OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            /// </summary>
            public const uint PROCESS_QUERY_INFORMATION = 0x0400;

            /// <summary>
            ///     Required to suspend or resume a process.
            /// </summary>
            public const uint PROCESS_SUSPEND_RESUME = 0x0800;

            /// <summary>
            ///     Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the
            ///     PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION.
            /// </summary>
            public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

            /// <summary>
            ///     All possible access rights for a process object.
            /// </summary>
            public const uint PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;
                //0x001F0FFF on WinXP, should be changed to 0xFFFF on Vista/2k8

            /// <summary>
            ///     Required to terminate a thread using TerminateThread.
            /// </summary>
            public const uint THREAD_TERMINATE = 0x0001;

            /// <summary>
            ///     Required to suspend or resume a thread.
            /// </summary>
            public const uint THREAD_SUSPEND_RESUME = 0x0002;

            /// <summary>
            ///     Required to read the context of a thread using <see cref="Imports.GetThreadContext" />
            /// </summary>
            public const uint THREAD_GET_CONTEXT = 0x0008;

            /// <summary>
            ///     Required to set the context of a thread using <see cref="Imports.SetThreadContext" />
            /// </summary>
            public const uint THREAD_SET_CONTEXT = 0x0010;

            /// <summary>
            ///     Required to read certain information from the thread object, such as the exit code (see GetExitCodeThread).
            /// </summary>
            public const uint THREAD_QUERY_INFORMATION = 0x0040;

            /// <summary>
            ///     Required to set certain information in the thread object.
            /// </summary>
            public const uint THREAD_SET_INFORMATION = 0x0020;

            /// <summary>
            ///     Required to set the impersonation token for a thread using SetThreadToken.
            /// </summary>
            public const uint THREAD_SET_THREAD_TOKEN = 0x0080;

            /// <summary>
            ///     Required to use a thread's security information directly without calling it by using a communication mechanism that
            ///     provides impersonation services.
            /// </summary>
            public const uint THREAD_IMPERSONATE = 0x0100;

            /// <summary>
            ///     Required for a server thread that impersonates a client.
            /// </summary>
            public const uint THREAD_DIRECT_IMPERSONATION = 0x0200;

            /// <summary>
            ///     All possible access rights for a thread object.
            /// </summary>
            public const uint THREAD_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3FF;
        }

        /// <summary>
        ///     Determines which registers are returned or set when using <see cref="Imports.GetThreadContext" /> or
        ///     <see cref="Imports.SetThreadContext" />.
        /// </summary>
        public static class CONTEXT_FLAGS
        {
            private const uint CONTEXT_i386 = 0x00010000;

            private const uint CONTEXT_i486 = 0x00010000;

            /// <summary>
            ///     SS:SP, CS:IP, FLAGS, BP
            /// </summary>
            public const uint CONTEXT_CONTROL = (CONTEXT_i386 | 0x01);

            /// <summary>
            ///     AX, BX, CX, DX, SI, DI
            /// </summary>
            public const uint CONTEXT_INTEGER = (CONTEXT_i386 | 0x02);

            /// <summary>
            ///     DS, ES, FS, GS
            /// </summary>
            public const uint CONTEXT_SEGMENTS = (CONTEXT_i386 | 0x04);

            /// <summary>
            ///     387 state
            /// </summary>
            public const uint CONTEXT_FLOATING_POINT = (CONTEXT_i386 | 0x08);

            /// <summary>
            ///     DB 0-3,6,7
            /// </summary>
            public const uint CONTEXT_DEBUG_REGISTERS = (CONTEXT_i386 | 0x10);

            /// <summary>
            ///     cpu specific extensions
            /// </summary>
            public const uint CONTEXT_EXTENDED_REGISTERS = (CONTEXT_i386 | 0x20);

            /// <summary>
            ///     Everything but extended information and debug registers.
            /// </summary>
            public const uint CONTEXT_FULL = (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS);

            /// <summary>
            ///     Everything.
            /// </summary>
            public const uint CONTEXT_ALL =
                (CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS);
        }

        /// <summary>
        ///     Values that determine how memory is allocated.
        /// </summary>
        public static class MemoryAllocType
        {
            /// <summary>
            ///     Allocates physical storage in memory or in the paging file on disk for the specified reserved memory pages. The
            ///     function initializes the memory to zero.
            ///     To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
            ///     The function fails if you attempt to commit a page that has not been reserved. The resulting error code is
            ///     ERROR_INVALID_ADDRESS.
            ///     An attempt to commit a page that is already committed does not cause the function to fail. This means that you can
            ///     commit pages without first determining the current commitment state of each page.
            /// </summary>
            public const uint MEM_COMMIT = 0x00001000;

            /// <summary>
            ///     Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or
            ///     in the paging file on disk.
            ///     You commit reserved pages by calling VirtualAllocEx again with MEM_COMMIT.
            /// </summary>
            public const uint MEM_RESERVE = 0x00002000;

            /// <summary>
            ///     Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages
            ///     should not be read from or written to the paging file. However, the memory block will be used again later, so it
            ///     should not be decommitted. This value cannot be used with any other value.
            ///     Using this value does not guarantee that the range operated on with MEM_RESET will contain zeroes. If you want the
            ///     range to contain zeroes, decommit the memory and then recommit it.
            /// </summary>
            public const uint MEM_RESET = 0x00080000;

            /// <summary>
            ///     Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
            ///     This value must be used with MEM_RESERVE and no other values.
            /// </summary>
            public const uint MEM_PHYSICAL = 0x00400000;

            /// <summary>
            ///     Allocates memory at the highest possible address.
            /// </summary>
            public const uint MEM_TOP_DOWN = 0x00100000;
        }

        /// <summary>
        ///     Values that determine how a block of memory is freed.
        /// </summary>
        public static class MemoryFreeType
        {
            /// <summary>
            ///     f
            ///     Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
            ///     The function does not fail if you attempt to decommit an uncommitted page. This means that you can decommit a range
            ///     of pages without first determining their current commitment state.
            ///     Do not use this value with MEM_RELEASE.
            /// </summary>
            public const uint MEM_DECOMMIT = 0x4000;

            /// <summary>
            ///     Releases the specified region of pages. After the operation, the pages are in the free state.
            ///     If you specify this value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the
            ///     VirtualAllocEx function when the region is reserved. The function fails if either of these conditions is not met.
            ///     If any pages in the region are committed currently, the function first decommits, and then releases them.
            ///     The function does not fail if you attempt to release pages that are in different states, some reserved and some
            ///     committed. This means that you can release a range of pages without first determining the current commitment state.
            ///     Do not use this value with MEM_DECOMMIT.
            /// </summary>
            public const uint MEM_RELEASE = 0x8000;
        }

        /// <summary>
        ///     Values that determine how a block of memory is protected.
        /// </summary>
        public static class MemoryProtectType
        {
            /// <summary>
            ///     Enables execute access to the committed region of pages. An attempt to read or write to the committed region
            ///     results in an access violation.
            /// </summary>
            public const uint PAGE_EXECUTE = 0x10;

            /// <summary>
            ///     Enables execute and read access to the committed region of pages. An attempt to write to the committed region
            ///     results in an access violation.
            /// </summary>
            public const uint PAGE_EXECUTE_READ = 0x20;

            /// <summary>
            ///     Enables execute, read, and write access to the committed region of pages.
            /// </summary>
            public const uint PAGE_EXECUTE_READWRITE = 0x40;

            /// <summary>
            ///     Enables execute, read, and write access to the committed region of image file code pages. The pages are shared
            ///     read-on-write and copy-on-write.
            /// </summary>
            public const uint PAGE_EXECUTE_WRITECOPY = 0x80;

            /// <summary>
            ///     Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed
            ///     region results in an access violation exception, called a general protection (GP) fault.
            /// </summary>
            public const uint PAGE_NOACCESS = 0x01;

            /// <summary>
            ///     Enables read access to the committed region of pages. An attempt to write to the committed region results in an
            ///     access violation. If the system differentiates between read-only access and execute access, an attempt to execute
            ///     code in the committed region results in an access violation.
            /// </summary>
            public const uint PAGE_READONLY = 0x02;

            /// <summary>
            ///     Enables both read and write access to the committed region of pages.
            /// </summary>
            public const uint PAGE_READWRITE = 0x04;

            /// <summary>
            ///     Gives copy-on-write protection to the committed region of pages.
            /// </summary>
            public const uint PAGE_WRITECOPY = 0x08;

            /// <summary>
            ///     Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a
            ///     STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard pages thus act as a one-time access
            ///     alarm. For more information, see Creating Guard Pages.
            ///     When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
            ///     If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
            ///     This value cannot be used with PAGE_NOACCESS.
            /// </summary>
            public const uint PAGE_GUARD = 0x100;

            /// <summary>
            ///     Does not allow caching of the committed regions of pages in the CPU cache. The hardware attributes for the physical
            ///     memory should be specified as "no cache." This is not recommended for general usage. It is useful for device
            ///     drivers, for example, mapping a video frame buffer with no caching.
            ///     This value cannot be used with PAGE_NOACCESS.
            /// </summary>
            public const uint PAGE_NOCACHE = 0x200;

            /// <summary>
            ///     Enables write-combined memory accesses. When enabled, the processor caches memory write requests to optimize
            ///     performance. Thus, if two requests are made to write to the same memory address, only the more recent write may
            ///     occur.
            ///     Note that the PAGE_GUARD and PAGE_NOCACHE flags cannot be specified with PAGE_WRITECOMBINE. If an attempt is made
            ///     to do so, the SYSTEM_INVALID_PAGE_PROTECTION NT error code is returned by the function.
            /// </summary>
            public const uint PAGE_WRITECOMBINE = 0x400;
        }

        #region Kernel32 Exports

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetSystemDirectory([Out] StringBuilder lpBuffer, uint uSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetHandleInformation(IntPtr hObject, out uint lpdwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetSystemWow64Directory([Out] StringBuilder lpBuffer, uint uSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool FreeLibrary(IntPtr h);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateThread(
            [In] ref SECURITY_ATTRIBUTES SecurityAttributes,
            uint StackSize,
            ThreadStart StartFunction,
            IntPtr ThreadParameter,
            uint CreationFlags,
            out uint ThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern uint GetThreadId(IntPtr hThread);

        [DllImport("user32.dll")]
        public static extern void MessageBeep(uint uType);

        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx")]
        public static extern IntPtr VirtualAllocEx([In] IntPtr hProcess, [In] IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx([In] IntPtr hProcess, [In] IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetModuleHandleA( /*IN*/ string lpModuleName);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetModuleHandleW( /*IN*/ string lpModuleName);

        [DllImport("kernel32.dll", EntryPoint = "IsWow64Process", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool Wow64Process);

        /// <summary>
        ///     ОЧЕНЬ, ОЧЕНЬ ПЛОХАЯ ФУНКЦИЯ, DO NOT USE _EVER_ !
        /// </summary>
        /// <param name="lpBase"></param>
        /// <param name="ucb"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "IsBadReadPtr")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool IsBadReadPtr(IntPtr lpBase, uint ucb);

        [DllImport("Dbghelp.dll", EntryPoint = "ImageDirectoryEntryToData")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr ImageDirectoryEntryToData(IntPtr Base, bool MappedAsImage, ushort DirectoryEntry, out uint Size);

        [DllImport("Dbghelp.dll", EntryPoint = "ImageNtHeader")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr ImageNtHeader(IntPtr Base);

        [DllImport("ImageHlp.dll", CallingConvention = CallingConvention.Winapi)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool MapAndLoad(string imageName, string dllPath, out LOADED_IMAGE loadedImage, bool dotDll, bool readOnly);

        [DllImport("Dbghelp.dll", CallingConvention = CallingConvention.Winapi)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr ImageRvaToVa(IntPtr pNtHeaders, IntPtr pBase, uint rva, IntPtr pLastRvaSection);

        [DllImport("psapi.dll", EntryPoint = "GetModuleFileNameExA", SetLastError = true)]
        public static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx // позволяет получить массив дескрипторов модулей
            (
            // у другого процесса
            IntPtr hProcess,
            // Дескриптор процесса, который содержит данный модуль
            IntPtr[] lphModules,
            // Массив дескрипторов полученных модулей
            int cb,
            // Размер lphModule массива в байтах
            out int lpcbNeeded,
            // Количество байт,
            // необходимых для хранения всех десткрипторов модуля в lphModule массиве
            int dwFilterFlag // 0x00 (LIST_MODULES_DEFAULT) - Use the default behavior
            );

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation // позволяет получить массив дескрипторов модулей
            (
            // у другого процесса
            IntPtr hProcess,
            // Дескриптор процесса, который содержит данный модуль
            IntPtr hModule,
            out _MODULEINFO ModuleInfo,
            int ModuleInfoInfoSize);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
        public static extern IntPtr GetModuleHandle([In()] [MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        #region ThreadFunctions

        /// <summary>
        ///     Creates a thread that runs in the virtual address space of another process.
        /// </summary>
        /// <param name="hProcess">A handle to the process in which the thread is to be created.</param>
        /// <param name="lpThreadAttributes">
        ///     A pointer to a SECURITY_ATTRIBUTES structure that specifies a security descriptor for
        ///     the new thread and determines whether child processes can inherit the returned handle. If lpThreadAttributes is
        ///     NULL, the thread gets a default security descriptor and the handle cannot be inherited.
        /// </param>
        /// <param name="dwStackSize">
        ///     The initial size of the stack, in bytes. The system rounds this value to the nearest page. If
        ///     this parameter is 0 (zero), the new thread uses the default size for the executable.
        /// </param>
        /// <param name="lpStartAddress">
        ///     A pointer to the application-defined function of type LPTHREAD_START_ROUTINE to be
        ///     executed by the thread and represents the starting address of the thread in the remote process. The function must
        ///     exist in the remote process.
        /// </param>
        /// <param name="lpParameter">A pointer to a variable to be passed to the thread function.</param>
        /// <param name="dwCreationFlags">The flags that control the creation of the thread.</param>
        /// <param name="dwThreadId">A pointer to a variable that receives the thread identifier.</param>
        /// <returns>
        ///     If the function succeeds, the return value is a handle to the new thread.  If the function fails, the return
        ///     value is IntPtr.Zero.
        /// </returns>
        [DllImport("kernel32", EntryPoint = "CreateRemoteThread")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            ThreadFlags dwCreationFlags,
            out IntPtr dwThreadId);

        /// <summary>
        ///     Waits until the specified object is in the signaled state or the time-out interval elapses.
        /// </summary>
        /// <param name="hObject">
        ///     A handle to the object. For a list of the object types whose handles can be specified, see the
        ///     following Remarks section.
        /// </param>
        /// <param name="dwMilliseconds">
        ///     The time-out interval, in milliseconds. The function returns if the interval elapses, even
        ///     if the object's state is nonsignaled. If dwMilliseconds is zero, the function tests the object's state and returns
        ///     immediately. If dwMilliseconds is INFINITE, the function's time-out interval never elapses.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value indicates the event that caused the function to return. If the
        ///     function fails, the return value is WAIT_FAILED ((DWORD)0xFFFFFFFF).
        /// </returns>
        [DllImport("kernel32", EntryPoint = "WaitForSingleObject")]
        public static extern WaitValues WaitForSingleObject(IntPtr hObject, uint dwMilliseconds);

        /// <summary>
        ///     Retrieves the termination status of the specified thread.
        /// </summary>
        /// <param name="hThread">A handle to the thread.</param>
        /// <param name="lpExitCode">[Out] The exit code of the thread.</param>
        /// <returns>A pointer to a variable to receive the thread termination status.For more information.</returns>
        [DllImport("kernel32", EntryPoint = "GetExitCodeThread")]
        public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

        /// <summary>
        ///     Opens an existing thread object.
        /// </summary>
        /// <param name="dwDesiredAccess">
        ///     The access to the thread object. This access right is checked against the security
        ///     descriptor for the thread. This parameter can be one or more of the thread access rights.
        /// </param>
        /// <param name="bInheritHandle">
        ///     If this value is TRUE, processes created by this process will inherit the handle.
        ///     Otherwise, the processes do not inherit this handle.
        /// </param>
        /// <param name="dwThreadId">The identifier of the thread to be opened.</param>
        /// <returns>
        ///     If the function succeeds, the return value is an open handle to the specified thread.
        ///     If the function fails, the return value is NULL.
        /// </returns>
        [DllImport("kernel32", EntryPoint = "OpenThread")]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        /// <summary>
        ///     Suspends execution of a given thread.
        /// </summary>
        /// <param name="hThread">Handle to the thread that will be suspended.</param>
        /// <returns>Returns (DWORD)-1 on failure, otherwise the suspend count of the thread.</returns>
        [DllImport("kernel32", EntryPoint = "SuspendThread")]
        public static extern uint SuspendThread(IntPtr hThread);

        /// <summary>
        ///     Resumes execution of a given thread.
        /// </summary>
        /// <param name="hThread">Handle to the thread that will be suspended.</param>
        /// <returns>Returns (DWORD)-1 on failure, otherwise the previous suspend count of the thread.</returns>
        [DllImport("kernel32", EntryPoint = "ResumeThread")]
        public static extern uint ResumeThread(IntPtr hThread);

        /// <summary>
        ///     Terminates the specified thread.
        /// </summary>
        /// <param name="hThread">Handle to the thread to exit.</param>
        /// <param name="dwExitCode">Exit code that will be stored in the thread object.</param>
        /// <returns>Returns zero on failure, non-zero on success.</returns>
        [DllImport("kernel32", EntryPoint = "TerminateThread")]
        public static extern uint TerminateThread(IntPtr hThread, uint dwExitCode);

        #endregion

        #endregion

        #region Public Constants

        public const int IMAGE_NT_SIGNATURE = 17744;

        public const int IMAGE_DOS_SIGNATURE = 23117;

        public const uint MB_OK = 0x00000000;

        public const uint MB_ICONHAND = 0x00000010;

        public const uint MB_ICONQUESTION = 0x00000020;

        public const uint MB_ICONEXCLAMATION = 0x00000030;

        public const uint MB_ICONASTERISK = 0x00000040;

        public static readonly ushort IMAGE_DIRECTORY_ENTRY_IMPORT = 1;

        public static readonly ushort IMAGE_DIRECTORY_ENTRY_EXPORT = 0;

        #endregion
    }
}