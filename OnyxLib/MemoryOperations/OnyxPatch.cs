#region Usings

using System;

#endregion

namespace OnyxLib.MemoryOperations
{
    public class OnyxPatch : MemoryOperationReversable
    {
        private readonly IntPtr m_hProcess;

        private readonly byte[] m_originalBytes;

        private readonly IntPtr m_patchAddress;

        private readonly byte[] m_patchBytes;

        /// <summary>
        ///     Initializes a new instance of the OnyxPatch class.
        /// </summary>
        /// <param name="_hProcess"></param>
        /// <param name="_patchAddress">Address of patch</param>
        /// <param name="_patchBytes">Bytes, that contains patch</param>
        /// <param name="_patchName">Patch name, that will be used in PatchManager</param>
        public OnyxPatch(IntPtr _hProcess, IntPtr _patchAddress, byte[] _patchBytes, string _patchName = null)
        {
            if (_patchName != null)
            {
                m_name = _patchName;
            }
            if (_hProcess == IntPtr.Zero)
            {
                throw new ArgumentNullException("_hProcess");
            }
            if (_patchAddress == IntPtr.Zero)
            {
                throw new ArgumentNullException("_patchAddress");
            }
            m_patchAddress = _patchAddress;
            m_patchBytes = _patchBytes;
            m_hProcess = _hProcess;
            m_originalBytes = OnyxMemory.ReadBytes(m_hProcess, m_patchAddress, m_patchBytes.Length);
        }

        /// <summary>
        ///     Restores bytes to their original values
        /// </summary>
        /// <returns>True if patch was unapplied</returns>
        public override bool Remove()
        {
            if (OnyxMemory.WriteBytes(m_hProcess, m_patchAddress, m_originalBytes))
            {
                IsApplied = false;
            }
            return !IsApplied;
        }

        /// <summary>
        ///     Applies patch
        /// </summary>
        /// <returns>True if patch was succcessfull</returns>
        public override bool Apply()
        {
            IsApplied = OnyxMemory.WriteBytes(m_hProcess, m_patchAddress, m_patchBytes);
            return IsApplied;
        }
    }
}