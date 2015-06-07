#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FasmWrapper
{
    internal static class FasmFunctions
    {
        /// <summary>
        ///     Fasm dll name
        /// </summary>
        public const string FasmLibraryName = "FASM.DLL";

        [DllImport(FasmLibraryName, CallingConvention = CallingConvention.StdCall, EntryPoint = "fasm_GetVersion")]
        public static extern uint Version();

        /// <summary>
        ///     Generates ASM byte code from ASM source code
        /// </summary>
        /// <param name="_szSource">ASM code lines, delimitered by \n</param>
        /// <param name="_buffer">
        ///     Output buffer, that will be filled by method. At the beginning there will be FasmState struct if
        ///     FASM_OK
        /// </param>
        /// <param name="_bufferSize">Output buffer length</param>
        /// <param name="_passesLimit">Number of compiler passes, MAX 0x10000, default is 100</param>
        /// <param name="_outputStreamHandle">Handle to output stream, may be IntPtr.Zero</param>
        /// <returns></returns>
        [DllImport(FasmLibraryName, CallingConvention = CallingConvention.StdCall, EntryPoint = "fasm_Assemble")]
        public static extern FasmResult Assemble(
            [MarshalAs(UnmanagedType.LPStr)] String _szSource,
            byte[] _buffer,
            int _bufferSize,
            int _passesLimit,
            IntPtr _outputStreamHandle);

        /// <summary>
        ///     Generates ASM byte code from ASM source code using default settings
        /// </summary>
        /// <param name="_source">ASM code lines, delimitered by \n</param>
        /// <param name="_buffer">Output buffer</param>
        /// <returns></returns>
        public static FasmResult Assemble(string _source, byte[] _buffer)
        {
            if (_source == null)
            {
                throw new ArgumentNullException("_source");
            }
            if (_buffer == null)
            {
                throw new ArgumentNullException("_buffer");
            }
            return Assemble(_source, _buffer, _buffer.Length, 100, IntPtr.Zero);
        }
    }
}