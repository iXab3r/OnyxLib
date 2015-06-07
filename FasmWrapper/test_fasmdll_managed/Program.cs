using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace test_fasmdll_managed
{
	class Program
	{
		[DllImport("kernel32")]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32")]
		private static extern uint VirtualAllocEx(IntPtr hProcess, uint dwAddress, int nSize, uint dwAllocationType, uint dwProtect);

		[DllImport("kernel32")]
		private static extern bool VirtualFreeEx(IntPtr hProcess, uint dwAddress, int nSize, uint dwFreeType);

		static void Main(string[] args)
		{
            try
            {
                var fasm = new FasmWrapper.Fasm();
                Console.WriteLine("Fasm");
                Console.WriteLine("Version: {0}", fasm.Version);

                var code = new List<string>();
                code.Add("push 7D1572h");
                code.Add("ret");
                byte[] asmCode = fasm.Assemble(code);
                foreach (byte b in asmCode)
                {
                    Console.Write(b.ToString("X2") + " ");
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
            finally
            {
                Console.ReadKey();
            }
            /*

			Fasm.ManagedFasm fasm = new Fasm.ManagedFasm();
			fasm.SetMemorySize(0x500);
			//fasm.AddLine("org " + dwBaseAddress.ToString("X")); //not necessary, .Inject does automatically
			fasm.AddLine("retn");
			fasm.AddLine("jmp 0x410000");
			fasm.AddLine("call 0x410000");
			byte[] a = fasm.Assemble();
            foreach (byte b in a)
            {
                Console.Write(b.ToString("X2")+" ");
            }*/
            Console.ReadKey();
		}
	}
}
