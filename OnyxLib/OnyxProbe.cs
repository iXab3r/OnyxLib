#region Usings

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using OnyxLib.MemoryOperations;

#endregion

namespace OnyxLib
{
    public class OnyxProbe : OnyxDetour
    {
        private uint codeCaveAddress_OnLeave = 0;

        public bool isApplied = false;

        public uint methodAddress;

        private Process process = Process.GetCurrentProcess();

        private uint retnAddress = 0;

        private Stopwatch sw = new Stopwatch();

        public OnyxProbe(IntPtr hProcess)
            : base(IntPtr.Zero, null)
        {
        }

        /// <summary>
        ///     Запускает зонд.
        ///     !!!!!!!!!!!!!!!!!!!! ЕСТЬ ОПАСНОСТЬ ПОТЕРИ ДАННЫХ В РЕГИСТРАХ, ОСОБЕННО В МУЛЬТИПОТОЧКЕ !!!!!!!!!!!!!!!!
        /// </summary>
        public override bool Apply()
        {
            var szCode = new StringBuilder(1024);

            // Кусок памяти для размещения переменной, содержащей адрес, с которого был вызван метод
            //retnAddress = BlackMagic.SMemory.AllocateMemory(process.Handle, sizeof(uint));

            var fasm = new RemoteFasm();
            fasm.AddLine("pushf");
            fasm.AddLine("pusha");
            fasm.AddLine("call {0}", Marshal.GetFunctionPointerForDelegate(new Action(OnLeave)).ToInt32());
            fasm.AddLine("popa");
            fasm.AddLine("popf");
            fasm.AddLine("push dword [{0}]", retnAddress);
            fasm.AddLine("retn");

            // Кусок памяти для размещения реакции на выход из функции
            fasm.Assemble();
            //codeCaveAddress_OnLeave = BlackMagic.SMemory.AllocateMemory(process.Handle, fasm.AssembledBytes.Length/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/);

            //!!!!!!!!!!!!fasm.Inject(codeCaveAddress_OnLeave);

            fasm.AddLine("pop dword [{0}]", retnAddress);
            fasm.AddLine("push {0}", codeCaveAddress_OnLeave);
            fasm.AddLine("call $+5");
            fasm.AddLine("sub dword [esp], {0}", /*!!!!!!!!!!!!!!!!!!!!!!!!*/fasm.Assemble().Length);
            fasm.AddLine("jmp dword {0}", Marshal.GetFunctionPointerForDelegate(new Action(OnEnter)).ToInt32());
            return true;
            /*
            _original = new List<byte>();
            _original.AddRange(BlackMagic.SMemory.ReadBytes(process.Handle, methodAddress, fasm.Assemble().Length));
            fasm.Inject(methodAddress);

            _new = new List<byte>();
            _new.AddRange(BlackMagic.SMemory.ReadBytes(process.Handle, methodAddress, _original.Count));
            */
        }

        public override bool Remove()
        {
            return true;
        }

        private void OnEnter()
        {
            Remove();
            sw.Restart();
        }

        private void OnLeave()
        {
            Apply();
            sw.Stop();
            MessageBox.Show(String.Format("Time elapsed: {0}", sw.ElapsedTicks));
        }

        ~OnyxProbe()
        {
            Dispose();
        }

        public new void Dispose()
        {
            if (isApplied)
            {
                Remove();
            }
            base.Dispose();
        }
    }
}