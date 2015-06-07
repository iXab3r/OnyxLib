#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace FasmWrapper
{
    /// <summary>
    ///     The following structure resides at the beginning of memory block provided
    ///     to the fasm_Assemble function. The condition field contains the same value
    ///     as the one returned by function.
    ///     When function returns FASM_OK condition, the output_length and
    ///     output_data fields are filled - with pointer to generated output
    ///     (somewhere within the provided memory block) and the count of bytes stored
    ///     there.
    ///     When function returns FASM_ERROR, the error_code is filled with the
    ///     code of specific error that happened and error_line is a pointer to the
    ///     LINE_HEADER structure, providing information about the line that caused
    ///     the error.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct FasmState
    {
        [FieldOffset(0)]
        public FasmResult Condition;

        [FieldOffset(4)]
        public FasmResult ErrorCode;

        [FieldOffset(4)]
        public uint OutputLength;

        [FieldOffset(8)]
        public IntPtr OutputData;

        [FieldOffset(8)]
        public IntPtr ErrorLine;

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Condition={0}, ErrorCode={1}", Condition, ErrorCode);
        }
    }
}