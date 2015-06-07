#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BeaEngine.Net;

using OnyxLib.Extensions;

#endregion

namespace OnyxLib.MemoryOperations
{
    public class OnyxDetour : MemoryOperationReversable
    {
        private readonly byte[] m_detourBytes;

        private readonly byte[] m_originalBytes;

        private readonly Delegate m_detour;
        private readonly Delegate m_originalFunction;

        private readonly IntPtr m_targetAddress;

        private readonly CheckThreadDelegate m_checkThread;

        public bool IgnoreThreadChecking = false;

        public HashSet<uint> ExcludedThreadId = new HashSet<uint>();

        private Disasm disasm;

        private IntPtr pJumperCodeCave;

        private delegate bool CheckThreadDelegate();

        /// <summary>
        ///     Initializes a new instance of the OnyxDetour class.
        /// </summary>
        public OnyxDetour(IntPtr _targetAddress, Delegate _detour, string _detourName = null)
        {
            if (_detourName != null)
            {
                m_name = _detourName;
            }
            if (_targetAddress == IntPtr.Zero)
            {
                throw new ArgumentNullException("_targetAddress");
            }
            if (_detour == null)
            {
                throw new ArgumentNullException("_detour");
            }

            m_detour = _detour;
            m_targetAddress = _targetAddress;
            IntPtr detourAddress = Marshal.GetFunctionPointerForDelegate(_detour);

            m_checkThread = CheckThread;
            var pCheckThread = Marshal.GetFunctionPointerForDelegate(m_checkThread);
            var fasm = new RemoteFasm();
            fasm.Clear();
            fasm.AddLine("@checkthread:");
            fasm.AddLine("call $+5");
            fasm.AddLine("add dword [esp], {0}", 5 + 4 + 1);
            fasm.AddLine("push 0{0:X}h", pCheckThread.ToInt64());
            fasm.AddLine("retn");
            fasm.AddLine("@checkthread_TestResults:");
            fasm.AddLine("test eax,eax");
            fasm.AddLine("jz @functionretn");
            fasm.AddLine("@userfunction:");
            fasm.AddLine("push 0{0:X}h", detourAddress.ToInt64());
            fasm.AddLine("retn");
            fasm.AddLine("@functionretn:");
            var jumperBytes = fasm.Assemble();
            pJumperCodeCave = Onyx.Instance.Memory.AllocateMemory(256);
            Onyx.Instance.Memory.WriteBytes(pJumperCodeCave, jumperBytes);

            fasm.Clear();
            fasm.AddLine("push 0{0:X}h", pJumperCodeCave.ToInt64());
            fasm.AddLine("retn");
            m_detourBytes = fasm.Assemble();

            disasm = new Disasm();
            var realEip = m_targetAddress;
            var bytesDisassembled = 0;
            while (bytesDisassembled < m_detourBytes.Length)
            {
                var managedInstructionBuffer = Onyx.Instance.Memory.ReadBytes(realEip, 15);
                var instructionBuffer = new UnmanagedBuffer(managedInstructionBuffer);
                disasm.EIP = instructionBuffer.Ptr;
                var length = BeaEngine32.Disasm(disasm);
                if ((length != (int)BeaConstants.SpecialInfo.OUT_OF_BLOCK) && (length != (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE))
                {
                    bytesDisassembled += length;
                    if ((disasm.Instruction.BranchType == (int)BeaConstants.BranchType.JmpType) && (disasm.Instruction.AddrValue != 0))
                    {
                        // jmp = modify EIP
                        //disasm.EIP = (IntPtr)disasm.Instruction.AddrValue;
                    }
                    realEip = (IntPtr)((ulong)realEip + (ulong)length);
                }
                else
                {
                    throw new Exception(String.Format("Disassembly error occured, exception code = {0}", length));
                }
            }

            //Store the orginal bytes of decoded instructions in memory after jumper code
            m_originalBytes = Onyx.Instance.Memory.ReadBytes(m_targetAddress, bytesDisassembled);
            var pOriginalFunction = pJumperCodeCave.Add((UInt32)jumperBytes.Length);
            Onyx.Instance.Memory.WriteBytes(pOriginalFunction, m_originalBytes);
            m_originalFunction = Marshal.GetDelegateForFunctionPointer(pOriginalFunction, _detour.GetType());

            var pReturnerCodeCave = pJumperCodeCave.Add((UInt32)jumperBytes.Length + (UInt32)bytesDisassembled);
            fasm.Clear();
            fasm.AddLine("push 0{0:X}h", m_targetAddress.Add((UInt32)bytesDisassembled).ToInt64());
            fasm.AddLine("retn");
            var returnerBytes = fasm.Assemble();
            Onyx.Instance.Memory.WriteBytes(pReturnerCodeCave, returnerBytes);

            // adding calling thread to exclusions
            Onyx.Instance.Detours.GlobalExcludedThreadId.Add(OnyxNative.GetCurrentThreadId());
        }

        public IntPtr JumperCodeCave
        {
            get
            {
                return pJumperCodeCave;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>True if hook is allowed to call UserFunction</returns>
        protected bool CheckThread()
        {
            if (IgnoreThreadChecking)
            {
                return true;
            }
            var currentThreadId = OnyxNative.GetCurrentThreadId(); // callee thread, probably should save GCID's original address on Onyx startup
            var result = !Onyx.Instance.Detours.GlobalExcludedThreadId.Contains(currentThreadId);
            return result;
        }

        /// <summary>
        ///     Restores bytes to their original values
        /// </summary>
        /// <returns>True if detour was removed</returns>
        public override bool Remove()
        {
            if (IsApplied && Onyx.Instance.Memory.WriteBytes(m_targetAddress, m_originalBytes))
            {
                IsApplied = false;
            }
            return !IsApplied;
        }

        /// <summary>
        ///     Applies detour
        /// </summary>
        /// <returns>True if detour was succcessfull</returns>
        public override bool Apply()
        {
            if (Onyx.Instance.Memory.WriteBytes(m_targetAddress, m_detourBytes))
            {
                IsApplied = true;
            }
            return IsApplied;
        }

        /// <summary>
        ///     Calls the original function, and returns a return value.
        /// </summary>
        /// <param name="args">
        ///     The arguments to pass. If it is a 'void' argument list,
        ///     you MUST pass 'null'.
        /// </param>
        /// <returns>An object containing the original functions return value.</returns>
        public object CallOriginal(params object[] args)
        {
            object ret = null;
            ret = m_originalFunction.DynamicInvoke(args);
            return ret;
        }

    }
}