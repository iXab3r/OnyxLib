#region Usings

using System;

#endregion

namespace OnyxLib.MemoryOperations
{
    public class OnyxRedirect : MemoryOperation
    {
        private readonly IntPtr m_detourAddress;

        private readonly byte[] m_detourBytes;

        private readonly byte[] _originalBytes;

        private readonly IntPtr m_targetAddress;

        /// <summary>
        ///     Initializes a new instance of the OnyxRedirect class.
        /// </summary>
        public OnyxRedirect(IntPtr targetAddress, IntPtr detourAddress, string detourName, byte[] _originalBytes)
        {
            if (detourName != null)
            {
                m_name = detourName;
            }
            if (targetAddress == IntPtr.Zero)
            {
                throw new ArgumentNullException("targetAddress");
            }
            if (detourAddress == IntPtr.Zero)
            {
                throw new ArgumentNullException("detourAddress");
            }

            m_targetAddress = targetAddress;
            m_detourAddress = detourAddress;
            this._originalBytes = _originalBytes;

            var fasm = new RemoteFasm();

            fasm.Clear();
            fasm.AddLine("push {0}", detourAddress);
            fasm.AddLine("retn");
            m_detourBytes = fasm.Assemble();
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
    }
}