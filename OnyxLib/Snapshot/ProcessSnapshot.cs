#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace OnyxLib.Snapshot
{
    public class ProcessSnapshot
    {
        public readonly IntPtr Handle;

        public readonly int Id;

        public readonly ProcessModuleSnapshot MainModule;

        public readonly List<ProcessModuleSnapshot> Modules;

        public readonly string ProcessName;

        public object Tag;

        public ProcessSnapshot(Process p)
        {
            try
            {
                ProcessName = p.ProcessName;
                Handle = p.Handle;
                Id = p.Id;

                MainModule = new ProcessModuleSnapshot(p.MainModule);
                Modules = new List<ProcessModuleSnapshot>();

                var lpcbNeeded = 0;
                IntPtr[] hModule = null;

                // Перечисляем 32/64-bit модули
                for (var moduleBit = 1; moduleBit <= 2; moduleBit++)
                {
                    OnyxNative.EnumProcessModulesEx(p.Handle, null, 0, out lpcbNeeded, moduleBit); // Пробуем получить lpcbNeeded
                    var arrSize = lpcbNeeded / IntPtr.Size; // получаем колличество модулей
                    hModule = new IntPtr[arrSize]; // Создаём массив нужного размера

                    if (OnyxNative.EnumProcessModulesEx(p.Handle, hModule, lpcbNeeded, out lpcbNeeded, moduleBit))
                    {
                        var nameFile = new StringBuilder(255); // Создаём массив, буфер из символов
                        for (var i = 0; i < arrSize; i++)
                        {
                            // Избегаем повторное добавление модулей (при перечислении для 64 bit процессов)
                            /*if (Modules.FindIndex(delegate(ProcessModuleSnapshot pms)
                            {
                                return pms.BaseAddress == hModule[i];
                            }) > 0) 
                                continue;*/
                            var Module = new ProcessModuleSnapshot();
                            OnyxNative.GetModuleFileNameEx(p.Handle, hModule[i], nameFile, nameFile.Capacity); // Пытаемся получить полный путь модуля
                            Module.ModuleName = Path.GetFileName(nameFile.ToString());
                            Module.FileName = nameFile.ToString();
                            Module.BaseAddress = hModule[i];
                            Module.InWhatProcess = this;
                            var mi = new _MODULEINFO();
                            if (OnyxNative.GetModuleInformation(p.Handle, hModule[i], out mi, Marshal.SizeOf(mi)))
                            {
                                Module.ModuleMemorySize = (int)mi.SizeOfImage;
                                Module.BaseAddress = mi.BaseOfDll;
                                Module.EntryPoint = mi.EntryPoint;
                            }
                            Module.is64bit = moduleBit == 2;
                            Modules.Add(Module);
                        }
                    }
                }
                /*
                foreach (ProcessModule pm in p.Modules)
                {
                    Modules.Add(new ProcessModuleSnapshot(pm,this));                    
                }*/
            }
            catch (Win32Exception)
            {
                Modules = null;
            }
            catch (InvalidOperationException)
            {
                Modules = null;
            }
        }

        public ProcessModuleSnapshot GetModuleByHandle(IntPtr Handle)
        {
            foreach (var pms in Modules)
            {
                if (pms.BaseAddress == Handle)
                {
                    return pms;
                }
            }
            return null;
        }
    }
}