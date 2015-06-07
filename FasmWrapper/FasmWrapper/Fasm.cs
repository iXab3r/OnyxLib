#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace FasmWrapper
{
    public class Fasm
    {
        /// <summary>
        ///     Buffer size for fasm_Assemble method
        /// </summary>
        public static int BufferSize = ushort.MaxValue;

        public Fasm()
        {
            LoadFasmDll();
        }

        /// <summary>
        ///     Used FASM version
        /// </summary>
        public Version Version
        {
            get
            {
                var fullVersion = FasmFunctions.Version();
                var minorVersion = fullVersion >> 16;
                var majorVersion = fullVersion & ((int)Math.Pow(2, 16) - 1);
                return new Version((int)majorVersion, (int)minorVersion);
            }
        }

        private void LoadFasmDll()
        {
            var hFasmDll = Native.LoadLibrary(FasmFunctions.FasmLibraryName);
            if (hFasmDll == IntPtr.Zero)
            {
                // extracting Fasm.dll from resources
                var dllBytes = ResourcesHelper.GetResourcesBytes(FasmFunctions.FasmLibraryName);
                if (dllBytes == null)
                {
                    throw new ApplicationException(string.Format("Could not extract library '{0}' from resources!", FasmFunctions.FasmLibraryName));
                }
                var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FasmFunctions.FasmLibraryName);
                File.WriteAllBytes(dllPath, dllBytes);

                hFasmDll = Native.LoadLibrary(dllPath);
                if (hFasmDll == IntPtr.Zero)
                {
                    var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new ApplicationException(string.Format("Could not load library '{0}' ! Error: {1}", FasmFunctions.FasmLibraryName, error));
                }
            }
        }

        private string HighlightString(string _input, int _lineNumber)
        {
            if (_input == null)
            {
                throw new ArgumentNullException("_input");
            }
            if (_lineNumber < 0)
            {
                throw new ArgumentOutOfRangeException("_lineNumber");
            }
            var stringsArray = BreakCodeLines(_input).ToArray();
            if (stringsArray.Count() < _lineNumber)
            {
                throw new ArgumentException(string.Format("Line number must be in range [1;{0}], LineNumber={1}", stringsArray.Length, _lineNumber));
            }
            stringsArray[_lineNumber - 1] = String.Format("ERROR > {0}", stringsArray[_lineNumber - 1]);
            return String.Join("\n", stringsArray);
        }

        /// <summary>
        ///     Generates bytes from ASM code lines, delimitered by \n
        /// </summary>
        /// <param name="_asmCode"></param>
        /// <returns></returns>
        public byte[] Assemble(string _asmCode)
        {
            if (_asmCode == null)
            {
                throw new ArgumentNullException("_asmCode");
            }
            _asmCode = CombineCodeLines(PrepareCode(_asmCode));

            var outputBuffer = new byte[BufferSize];
            var result = FasmFunctions.Assemble(_asmCode, outputBuffer);
            if (result == FasmResult.FASM_OK)
            {
                var state = FromByteArray<FasmState>(outputBuffer);
                if (state.Condition != FasmResult.FASM_OK)
                {
                    var expectionMsg = new StringBuilder();
                    expectionMsg.AppendLine(string.Format("FasmResult = {0}, FasmState = {1}", result, state));
                    throw new Exception(expectionMsg.ToString());
                }
                var asmCodeBuffer = new byte[state.OutputLength];
                Marshal.Copy(state.OutputData, asmCodeBuffer, 0, asmCodeBuffer.Length);
                return asmCodeBuffer;
            } else
            {
                var expectionMsg = new StringBuilder();
                expectionMsg.AppendLine(string.Format("FasmResult = {0}", result));
                if (result == FasmResult.FASM_ERROR)
                {
                    var state = FromByteArray<FasmState>(outputBuffer);
                    expectionMsg.AppendLine(String.Format("State = {0}", state));
                    if (state.ErrorLine == IntPtr.Zero)
                    {
                        expectionMsg.AppendLine(String.Format("ErrorLine is null"));
                    } else
                    {
                        var lineHeader = (FasmLineHeader)Marshal.PtrToStructure(state.ErrorLine, typeof(FasmLineHeader));
                        expectionMsg.AppendLine(String.Format("ErrorLine = {0}", lineHeader));
                        expectionMsg.AppendLine(HighlightString(_asmCode, (int)lineHeader.LineNumber));
                    }
                }
                throw new Exception(expectionMsg.ToString());
            }
        }

        private IEnumerable<string> BreakCodeLines(string _inputCode)
        {
            if (_inputCode == null)
            {
                throw new ArgumentNullException("_inputCode");
            }
            return _inputCode.Split(new[] { "\n", }, StringSplitOptions.None);
        }

        private string CombineCodeLines(IEnumerable<string> _codeLines)
        {
            if (_codeLines == null)
            {
                throw new ArgumentNullException("_codeLines");
            }
            return String.Join("\n", _codeLines);
        }

        private IEnumerable<string> PrepareCode(string _code)
        {
            if (_code == null)
            {
                throw new ArgumentNullException("_code");
            }
            return PrepareCode(BreakCodeLines(_code));
        }

        private IEnumerable<string> PrepareCode(IEnumerable<string> _code)
        {
            if (_code == null)
            {
                throw new ArgumentNullException("_code");
            }
            _code = _code.Where(x => x != null);
            _code = _code.Select(x => x.Trim());
            if (!_code.Any(x => "use32".Equals(x) || "use64".Equals(x)))
            {
                _code = new[] { "use32" }.Concat(_code);
            }
            return _code.ToArray();
        }

        /// <summary>
        ///     Generates bytes from ASM code lines
        /// </summary>
        /// <param name="_asmCode"></param>
        /// <returns></returns>
        public byte[] Assemble(IEnumerable<string> _asmCode)
        {
            if (_asmCode == null)
            {
                throw new ArgumentNullException("_asmCode");
            }
            var inputCode = CombineCodeLines(_asmCode);
            return Assemble(inputCode);
        }

        private static T FromByteArray<T>(byte[] _data)
        {
            if (_data == null)
            {
                throw new ArgumentNullException("_data");
            }
            // Pin the managed memory while, copy it out the data, then unpin it
            var handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
            var theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
    }
}