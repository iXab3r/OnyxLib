#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace OnyxLib
{
    /// <summary>
    ///    Class, that simplifies working with process memory
    /// </summary>
    public class OnyxMemory : IDisposable
    {
        private readonly IntPtr m_hOpenedProcess;

        /// <summary>
        ///   Handle of opened process
        /// </summary>
        public IntPtr OpenedProcess
        {
            get
            {
                return m_hOpenedProcess;
            }
        }

        public OnyxMemory(int _pid)
        {
            m_hOpenedProcess = OnyxNative.OpenProcess(0x001F0FFF, true, _pid);
        }

        public OnyxMemory() : this(Process.GetCurrentProcess().Id)
        {
        }

        /// <summary>
        ///     Closes opened process Handle
        /// </summary>
        public void Dispose()
        {
            OnyxNative.CloseHandle(m_hOpenedProcess);
        }

        /// <summary>
        ///     Writes byte array to destination address
        /// </summary>
        /// <param name="_hProcess">Opened process handle</param>
        /// <param name="_targetAddress">Address to write to</param>
        /// <param name="_bytes">Value to write</param>
        /// <returns></returns>
        public static bool WriteBytes(IntPtr _hProcess, IntPtr _targetAddress, byte[] _bytes)
        {
            if (_bytes == null)
            {
                throw new ArgumentNullException("_bytes");
            }
            if (_hProcess == IntPtr.Zero)
            {
                throw new Exception("Target process handle == IntPtr.Zero, open process first");
            }

            var methodDesc = String.Format(
                "WriteBytes(IntPtr 0x{0:X}, IntPtr 0x:{1:X}, byte[] {2}b)",
                _hProcess.ToInt64(),
                _targetAddress.ToInt64(),
                _bytes.Length);

            int written;
            if (OnyxNative.WriteProcessMemory(_hProcess, _targetAddress, _bytes, (uint)_bytes.Length, out written))
            {
                if (written != _bytes.Length)
                {
                    throw new ApplicationException(string.Format("{2} - Could not write all {0} byte(s), only {1} were written", _bytes.Length, written, methodDesc));
                }
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), methodDesc);
            }
            return true;
        }

        public bool WriteBytes(IntPtr _targetAddress, byte[] _bytes)
        {
            return WriteBytes(m_hOpenedProcess, _targetAddress, _bytes);
        }

        /// <summary>
        ///     Writes a generic datatype to memory. (Note; only base datatypes are supported)
        ///     [int,float,uint,byte,sbyte,double,byte[],string])
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_targetAddress">Address to write to</param>
        /// <param name="_value">Value to write</param>
        /// <param name="_hProcess">Opened process handle</param>
        /// <returns></returns>
        public static bool Write<T>(IntPtr _hProcess, IntPtr _targetAddress, T _value)
        {
            try
            {
                object val = _value; //boxing
                byte[] bytes;

                // Make sure we're handling passing in stuff as a byte array.
                if (typeof(T) == typeof(byte[]))
                {
                    bytes = (byte[])val;
                    return WriteBytes(_hProcess, _targetAddress, bytes);
                }
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        bytes = BitConverter.GetBytes((bool)val);
                        break;
                    case TypeCode.Char:
                        bytes = BitConverter.GetBytes((char)val);
                        break;
                    case TypeCode.Byte:
                        bytes = new[] { (byte)val };
                        break;
                    case TypeCode.Int16:
                        bytes = BitConverter.GetBytes((short)val);
                        break;
                    case TypeCode.UInt16:
                        bytes = BitConverter.GetBytes((ushort)val);
                        break;
                    case TypeCode.Int32:
                        bytes = BitConverter.GetBytes((int)val);
                        break;
                    case TypeCode.UInt32:
                        bytes = BitConverter.GetBytes((uint)val);
                        break;
                    case TypeCode.Int64:
                        bytes = BitConverter.GetBytes((long)val);
                        break;
                    case TypeCode.UInt64:
                        bytes = BitConverter.GetBytes((ulong)val);
                        break;
                    case TypeCode.Single:
                        bytes = BitConverter.GetBytes((float)val);
                        break;
                    case TypeCode.Double:
                        bytes = BitConverter.GetBytes((double)val);
                        break;
                    case TypeCode.String:
                        bytes = new UnicodeEncoding().GetBytes((string)val);
                        break;
                    default:
                        throw new NotSupportedException(typeof(T).FullName + " is not currently supported by Write<T>");
                }
                return WriteBytes(_hProcess, _targetAddress, bytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Writes a generic datatype to current process memory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_targetAddress"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        public bool Write<T>(IntPtr _targetAddress, T _value)
        {
            return Write(m_hOpenedProcess, _targetAddress, _value);
        }

        /// <summary>
        ///   Reads _count bytes from specified process memory
        /// </summary>
        /// <param name="_hProcess">Opened process handle</param>
        /// <param name="_targetAddress">Address to read from</param>
        /// <param name="_count">Bytes count to read</param>
        /// <returns></returns>
        public static byte[] ReadBytes(IntPtr _hProcess, IntPtr _targetAddress, int _count)
        {
            if (_hProcess == IntPtr.Zero)
            {
                throw new Exception("Target process handle == IntPtr.Zero, open process first");
            }
            var ret = new byte[_count];
            int numRead;
            if (OnyxNative.ReadProcessMemory(_hProcess, _targetAddress, ret, _count, out numRead) && numRead == _count)
            {
                return ret;
            } else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public byte[] ReadBytes(IntPtr _targetAddress, int _count)
        {
            return ReadBytes(m_hOpenedProcess, _targetAddress, _count);
        }

        /// <summary>
        ///     Reads a generic type from target process' memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_hProcess"></param>
        /// <param name="_address"></param>
        /// <returns></returns>
        public static T Read<T>(IntPtr _hProcess, IntPtr _address)
        {
            object ret;

            var size = Marshal.SizeOf(typeof(T));
            var ba = ReadBytes(_hProcess, _address, size);

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    ret = BitConverter.ToBoolean(ba, 0);
                    break;
                case TypeCode.Char:
                    ret = BitConverter.ToChar(ba, 0);
                    break;
                case TypeCode.Byte:
                    ret = ba[0];
                    break;
                case TypeCode.Int16:
                    ret = BitConverter.ToInt16(ba, 0);
                    break;
                case TypeCode.UInt16:
                    ret = BitConverter.ToUInt16(ba, 0);
                    break;
                case TypeCode.Int32:
                    ret = BitConverter.ToInt32(ba, 0);
                    break;
                case TypeCode.UInt32:
                    ret = BitConverter.ToUInt32(ba, 0);
                    break;
                case TypeCode.Int64:
                    ret = BitConverter.ToInt64(ba, 0);
                    break;
                case TypeCode.UInt64:
                    ret = BitConverter.ToUInt64(ba, 0);
                    break;
                case TypeCode.Single:
                    ret = BitConverter.ToSingle(ba, 0);
                    break;
                case TypeCode.Double:
                    ret = BitConverter.ToDouble(ba, 0);
                    break;
                default:
                    throw new NotSupportedException(typeof(T).FullName + " is not currently supported by Read<T>");
            }
            return (T)ret;
        }

        /// <summary>
        ///     Reads a generic type from current process' memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_targetAddress"></param>
        /// <returns></returns>
        public T Read<T>(IntPtr _targetAddress)
        {
            return (T)Read<T>(m_hOpenedProcess, _targetAddress);
        }

        /// <summary>
        ///     Allocates a block of memory in the target process.
        /// </summary>
        /// <param name="_hProcess">Handle to the process in which memory will be allocated.</param>
        /// <param name="_bytesCount">Number of bytes to be allocated.  Default is 0x1000.</param>
        /// <param name="_allocationType">The type of memory allocation.  See <see cref="OnyxNative.MemoryAllocType" /></param>
        /// <param name="_protectType">
        ///     The memory protection for the region of pages to be allocated. If the pages are being
        ///     committed, you can specify any one of the <see cref="OnyxNative.MemoryProtectType" /> constants.
        /// </param>
        /// <returns>Returns zero on failure, or the base address of the allocated block of memory on success.</returns>
        public static IntPtr AllocateMemory(IntPtr _hProcess, uint _bytesCount, uint _allocationType, uint _protectType)
        {
            var result = OnyxNative.VirtualAllocEx(_hProcess, IntPtr.Zero, _bytesCount, _allocationType, _protectType);
            if (result == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return result;
        }

        /// <summary>
        ///     Allocates a block of memory in the target process.
        /// </summary>
        /// <param name="_hProcess">Handle to the process in which memory will be allocated.</param>
        /// <param name="_bytesCount">Number of bytes to be allocated.  Default is 0x1000.</param>
        /// <returns>Returns zero on failure, or the base address of the allocated block of memory on success.</returns>
        /// <remarks>
        ///     Uses <see cref="OnyxNative.MemoryAllocType.MEM_COMMIT" /> for allocation type and
        ///     <see cref="OnyxNative.MemoryProtectType.PAGE_EXECUTE_READWRITE" /> for protect type.
        /// </remarks>
        public static IntPtr AllocateMemory(IntPtr _hProcess, uint _bytesCount = 1000)
        {
            return AllocateMemory(_hProcess, _bytesCount, OnyxNative.MemoryAllocType.MEM_COMMIT, OnyxNative.MemoryProtectType.PAGE_EXECUTE_READWRITE);
        }

        /// <summary>
        ///     Allocates a block of memory in current process.
        /// </summary>
        /// <param name="_bytesCount">Number of bytes to be allocated.  Default is 0x1000.</param>
        /// <returns>Returns zero on failure, or the base address of the allocated block of memory on success.</returns>
        /// <remarks>
        ///     Uses <see cref="OnyxNative.MemoryAllocType.MEM_COMMIT" /> for allocation type and
        ///     <see cref="OnyxNative.MemoryProtectType.PAGE_EXECUTE_READWRITE" /> for protect type.
        /// </remarks>
        public IntPtr AllocateMemory(uint _bytesCount = 1000)
        {
            return AllocateMemory(m_hOpenedProcess, _bytesCount, OnyxNative.MemoryAllocType.MEM_COMMIT, OnyxNative.MemoryProtectType.PAGE_EXECUTE_READWRITE);
        }

        /// <summary>
        ///     Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified
        ///     process.
        /// </summary>
        /// <param name="_hProcess">Handle to the process in which memory will be freed.</param>
        /// <param name="_memAddress">A pointer to the starting address of the region of memory to be freed. </param>
        /// <param name="_bytesCount">
        ///     The size of the region of memory to free, in bytes.
        ///     If the dwFreeType parameter is MEM_RELEASE, dwSize must be 0 (zero). The function frees the entire region that is
        ///     reserved in the initial allocation call to VirtualAllocEx.
        /// </param>
        /// <param name="_freeType">The type of free operation.  See <see cref="OnyxNative.MemoryFreeType" />.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public static bool FreeMemory(IntPtr _hProcess, IntPtr _memAddress, uint _bytesCount, uint _freeType)
        {
            if (_freeType == OnyxNative.MemoryFreeType.MEM_RELEASE)
            {
                _bytesCount = 0;
            }
            return OnyxNative.VirtualFreeEx(_hProcess, _memAddress, _bytesCount, _freeType);
        }

        /// <summary>
        ///     Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified
        ///     process.
        /// </summary>
        /// <param name="_hProcess">Handle to the process in which memory will be freed.</param>
        /// <param name="dwAddress">A pointer to the starting address of the region of memory to be freed. </param>
        /// <returns>Returns true on success, false on failure.</returns>
        /// <remarks>
        ///     Uses <see cref="OnyxNative.MemoryFreeType.MEM_RELEASE" /> to free the page(s) specified.
        /// </remarks>
        public static bool FreeMemory(IntPtr _hProcess, IntPtr _memAddress)
        {
            return FreeMemory(_hProcess, _memAddress, 0, OnyxNative.MemoryFreeType.MEM_RELEASE);
        }

        /// <summary>
        ///     Releases, decommits, or releases and decommits a region of memory within the virtual address space of current
        ///     process.
        /// </summary>
        /// <param name="_memAddress">A pointer to the starting address of the region of memory to be freed. </param>
        /// <returns>Returns true on success, false on failure.</returns>
        /// <remarks>
        ///     Uses <see cref="OnyxNative.MemoryFreeType.MEM_RELEASE" /> to free the page(s) specified.
        /// </remarks>
        public bool FreeMemory(IntPtr _memAddress)
        {
            return FreeMemory(m_hOpenedProcess, _memAddress, 0, OnyxNative.MemoryFreeType.MEM_RELEASE);
        }
    }
}