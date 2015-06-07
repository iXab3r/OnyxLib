using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using XMLib;
using XMLib.Log;

namespace OnyxLib
{
    public class OnyxHelpers
    {
        /// <summary>
        ///   CLSID for CLR runtime host (from mscoree.h)
        /// </summary>
        private static readonly byte[] CLSID_CLRRuntimeHost = new byte[] { 0x6E, 0xA0, 0xF1, 0x90, 0x12, 0x77, 0x62, 0x47, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02 };

        /// <summary>
        /// IID for ICLR runtime host (from mscoree.h)
        /// </summary>
        private static readonly byte[] IID_ICLRRuntimeHost = new byte[] { 0x6C, 0xA0, 0xF1, 0x90, 0x12, 0x77, 0x62, 0x47, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02 };


        private static readonly string MSCOREE_DLL_NAME = "mscoree.dll";
        private static readonly string MSCOREE_DLL_NAME_32 = Path.Combine(OnyxNative.GetSystemDirectory(), MSCOREE_DLL_NAME);
        private static readonly string MSCOREE_DLL_NAME_64 = Path.Combine(OnyxNative.GetSystemWow64Directory(), MSCOREE_DLL_NAME);


        /// <summary>
        ///     Injects a dll into a process by creating a remote thread on LoadLibrary.
        /// </summary>
        /// <param name="_hProcess">Handle to the process into which dll will be injected.</param>
        /// <param name="_szDllPath">Full path of the dll that will be injected.</param>
        /// <returns>Returns the base address of the injected dll on success, zero on failure.</returns>
        public static IntPtr InjectDllCreateThread(IntPtr _hProcess, string _szDllPath)
        {
            if (_hProcess == IntPtr.Zero)
            {
                throw new ArgumentNullException("_hProcess");
            }
            if (_szDllPath.Length == 0)
            {
                throw new ArgumentNullException("_szDllPath");
            }
            if (!_szDllPath.Contains("\\"))
            {
                _szDllPath = Path.GetFullPath(_szDllPath);
            }
            if (!File.Exists(_szDllPath))
            {
                throw new ArgumentException("DLL not found.", "_szDllPath");
            }
            var dwBaseAddress = IntPtr.Zero;
            IntPtr lpLoadLibrary;
            IntPtr lpDll;
            IntPtr hThread, threadId;
            var hKernel32 = OnyxNative.GetModuleHandle(_hProcess, "kernel32.dll");
            lpLoadLibrary =
                (IntPtr)(hKernel32.ToInt64() + OnyxNative.GetExportedFunctionRVA(OnyxNative.GetModuleFileNameEx(_hProcess, hKernel32), "LoadLibraryW").ToInt64());
            if (lpLoadLibrary != IntPtr.Zero)
            {
                lpDll = OnyxMemory.AllocateMemory(_hProcess);
                if (lpDll != IntPtr.Zero)
                {
                    if (OnyxMemory.Write(_hProcess, lpDll, _szDllPath))
                    {
                        //wait for thread handle to have signaled state
                        hThread = OnyxNative.CreateRemoteThread(
                            _hProcess,
                            IntPtr.Zero,
                            0,
                            lpLoadLibrary,
                            lpDll,
                            ThreadFlags.THREAD_EXECUTE_IMMEDIATELY,
                            out threadId);
                        //wait for thread handle to have signaled state
                        //exit code will be equal to the base address of the dll
                        if (OnyxNative.WaitForSingleObject(hThread, 5000) == WaitValues.WAIT_OBJECT_0)
                        {
                            OnyxNative.GetExitCodeThread(hThread, out dwBaseAddress);
                            if (dwBaseAddress == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }
                        OnyxNative.CloseHandle(hThread);
                    }
                    OnyxMemory.FreeMemory(_hProcess, lpDll);
                }
            }
            return dwBaseAddress;
        }


        //ExecuteInDefaultAppDomain(_processId, Path.GetDirectoryName(Application.ExecutablePath) + "\\" + DLLName, "Onyx.DomainManager.EntryPoint", "Main", "");
        /// <summary>
        ///   Loads CLR and target assembly into process address space
        /// </summary>
        /// <param name="_processId">Target process</param>
        /// <param name="_assemblyPath">Target assembly</param>
        /// <param name="_typeName">Type that contains method, that will be executed</param>
        /// <param name="_methodName">Method, that will be executed upon load</param>
        /// <param name="_args">Arguments, that will be passed to method</param>
        public static void InjectAndExecuteInDefaultAppDomain(Int32 _processId, String _assemblyPath, String _typeName, String _methodName, String _args)
        {
	        if (_assemblyPath == null)
	        {
		        throw new ArgumentNullException(nameof(_assemblyPath));
	        }
	        if (_typeName == null)
	        {
		        throw new ArgumentNullException(nameof(_typeName));
	        }
	        if (_methodName == null)
	        {
		        throw new ArgumentNullException(nameof(_methodName));
	        }

	        Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Injecting assembly '{0}'({1}.{2}, args '{3}') into process {4}...", _assemblyPath, _typeName, _methodName, _args ?? "null", _processId);
            var is64BitProcess = OnyxNative.Is64bitProcess(_processId);
            var clrDllPath = is64BitProcess ? MSCOREE_DLL_NAME_64 : MSCOREE_DLL_NAME_32;
            try
            {
                using (var onyx = new Onyx(_processId))
                {
                    // проверяем, не загружена ли уже CLR 
                    var hRemoteClr = OnyxNative.GetModuleHandle(onyx.Memory.OpenedProcess, MSCOREE_DLL_NAME);
                    if (hRemoteClr != IntPtr.Zero)
                    {
                        Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] MSCOREE.dll({1}) is already exists in process {0}...", _processId, clrDllPath);
                    } else
                    {
                        Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Loading MSCOREE.dll({1}) into process {0}...", _processId, clrDllPath);
                        InjectDllCreateThread(onyx.Memory.OpenedProcess, clrDllPath);
                        hRemoteClr = OnyxNative.GetModuleHandle(onyx.Memory.OpenedProcess, MSCOREE_DLL_NAME);
                        if (hRemoteClr == IntPtr.Zero)
                        {
                            throw new ApplicationException(String.Format("Could not load dll '{0}' into process {1}", clrDllPath, _processId));
                        }
                    }
                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] MSCOREE.dll({1}) handle - 0x{0:X8}", hRemoteClr.ToInt64(), clrDllPath);

                    var clrModuleName = OnyxNative.GetModuleFileNameEx(onyx.Memory.OpenedProcess, hRemoteClr);
                    var bindToRuntimeFuncRva = OnyxNative.GetExportedFunctionRVA(clrModuleName, "CorBindToRuntimeEx");
                    var lpCorBindToRuntimeEx = (IntPtr)(hRemoteClr.ToInt64() + bindToRuntimeFuncRva.ToInt64());
                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] CorBindToRuntimeEx ptr -> 0x{0:X8}", lpCorBindToRuntimeEx.ToInt64());
                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Allocating code caves...");
                    var lpCLSID_CLRRuntimeHost = onyx.Memory.AllocateMemory((uint)(CLSID_CLRRuntimeHost.Length * 4));
                    var lpIID_ICLRRuntimeHost = onyx.Memory.AllocateMemory((uint)IID_ICLRRuntimeHost.Length);
                    var lpClrHost = onyx.Memory.AllocateMemory(0x4);
                    var lpdwRet = onyx.Memory.AllocateMemory(0x4);
                    var lpCodeCave = onyx.Memory.AllocateMemory(0x256);
                    var lpAssemblyPath = onyx.Memory.AllocateMemory((uint)_assemblyPath.Length + 1);
                    var lpTypeName = onyx.Memory.AllocateMemory((uint)_typeName.Length + 1);
                    var lpMethodName = onyx.Memory.AllocateMemory((uint)_methodName.Length + 1);
                    var lpArgs = onyx.Memory.AllocateMemory((uint)_args.Length + 1);
                    var lpBuildFlavor = onyx.Memory.AllocateMemory((uint)BuildFlavor.Length * 2 + 2);
                    var lpFrameworkVersion = onyx.Memory.AllocateMemory((uint)FrameworkVersion.Length * 2 + 1);
                    onyx.Memory.Write(lpBuildFlavor, BuildFlavor);
                    onyx.Memory.Write(lpAssemblyPath, _assemblyPath);
                    onyx.Memory.Write(lpTypeName, _typeName);
                    onyx.Memory.Write(lpMethodName, _methodName);
                    onyx.Memory.Write(lpArgs, _args);
                    onyx.Memory.Write(lpCLSID_CLRRuntimeHost, CLSID_CLRRuntimeHost);
                    onyx.Memory.Write(lpIID_ICLRRuntimeHost, IID_ICLRRuntimeHost);
                    onyx.Memory.Write(lpFrameworkVersion, FrameworkVersion);

                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Preparing ASM code...");
                    var fasm = new RemoteFasm();
                    fasm.AddLine("use32");
                    fasm.AddLine("push {0}", lpClrHost.ToInt64());
                    fasm.AddLine("push {0}", lpIID_ICLRRuntimeHost.ToInt64());
                    fasm.AddLine("push {0}", lpCLSID_CLRRuntimeHost.ToInt64());
                    fasm.AddLine("push 0");
                    fasm.AddLine("push {0}", lpBuildFlavor.ToInt64());
                    fasm.AddLine("push {0}", lpFrameworkVersion.ToInt64());
                    fasm.AddLine("mov eax, {0}", lpCorBindToRuntimeEx.ToInt64());
                    fasm.AddLine("call eax"); // CorBindToRuntimeEx ()
                    fasm.AddLine("mov eax, [{0}]", lpClrHost.ToInt64());
                    fasm.AddLine("mov ecx, [eax]");
                    fasm.AddLine("mov edx, [ecx+0xC]");
                    fasm.AddLine("push eax");
                    fasm.AddLine("call edx"); // ClrHost ()
                    fasm.AddLine("push {0}", lpdwRet.ToInt64());
                    fasm.AddLine("push {0}", lpArgs.ToInt64());
                    fasm.AddLine("push {0}", lpMethodName.ToInt64());
                    fasm.AddLine("push {0}", lpTypeName.ToInt64());
                    fasm.AddLine("push {0}", lpAssemblyPath.ToInt64());
                    fasm.AddLine("mov eax, [{0}]", lpClrHost.ToInt64());
                    fasm.AddLine("mov ecx, [eax]");
                    fasm.AddLine("push eax");
                    fasm.AddLine("mov eax, [ecx+0x2C]");
                    fasm.AddLine("call eax");
                    fasm.AddLine("retn");

                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Injecting and executing ASM code...");
                    fasm.InjectAndExecute(onyx.Memory.OpenedProcess, lpCodeCave);

                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Disposing code caves...");
                    onyx.Memory.FreeMemory(lpCLSID_CLRRuntimeHost);
                    onyx.Memory.FreeMemory(lpIID_ICLRRuntimeHost);
                    onyx.Memory.FreeMemory(lpClrHost);
                    onyx.Memory.FreeMemory(lpdwRet);
                    onyx.Memory.FreeMemory(lpCodeCave);
                    onyx.Memory.FreeMemory(lpAssemblyPath);
                    onyx.Memory.FreeMemory(lpTypeName);
                    onyx.Memory.FreeMemory(lpMethodName);
                    onyx.Memory.FreeMemory(lpArgs);
                    onyx.Memory.FreeMemory(lpBuildFlavor);
                    onyx.Memory.FreeMemory(lpFrameworkVersion);

                    Logger.InfoFormat("[InjectAndExecuteInDefaultAppDomain] Assembly sussessfully injected");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Could not inject assembly '{0}'({1}.{2}, args '{3}') into process {4}", _assemblyPath, _typeName, _methodName, _args ?? "null", _processId), ex);
            }
        }

        /// <summary>
        /// [in] A string describing the version of the CLR you want to load.
        /// A version number in the .NET Framework consists of four parts separated by periods: major.minor.build.revision. The string passed as pwszVersion must start with the character "v" followed by the first three parts of the version number (for example, "v1.0.1529").
        /// Some versions of the CLR are installed with a policy statement that specifies compatibility with previous versions of the CLR. By default, the startup shim evaluates pwszVersion against policy statements and loads the latest version of the runtime that is compatible with the version being requested. A host can force the shim to skip policy evaluation and load the exact version specified in pwszVersion by passing a value of STARTUP_LOADER_SAFEMODE for the startupFlags parameter, as described below.
        /// If the caller specifies null for pwszVersion, CorBindToRuntimeEx identifies the set of installed runtimes whose version numbers are lower than the .NET Framework 4 runtime, and loads the latest version of the runtime from that set. It won't load the .NET Framework 4 or later, and fails if no earlier version is installed. Note that passing null gives the host no control over which version of the runtime is loaded. Although this approach may be appropriate in some scenarios, it is strongly recommended that the host supply a specific version to load.
        /// </summary>
        public static string FrameworkVersion = "v4.0.30319";// "v2.0.50727";

        /// <summary>
        /// [in] A string that specifies whether to load the server or the workstation build of the CLR. Valid values are svr and wks. The server build is optimized to take advantage of multiple processors for garbage collections, and the workstation build is optimized for client applications running on a single-processor machine.
        /// If pwszBuildFlavoris set to null, the workstation build is loaded. When running on a single-processor machine, the workstation build is always loaded, even if pwszBuildFlavoris set to svr. However, if pwszBuildFlavoris set to svr and concurrent garbage collection is specified (see the description of the startupFlags parameter), the server build is loaded.
        /// </summary>
        public static string BuildFlavor = "wks";
    }
}
