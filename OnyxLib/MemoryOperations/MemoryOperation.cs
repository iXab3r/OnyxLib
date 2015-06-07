#region Usings

using System;

#endregion

namespace OnyxLib.MemoryOperations
{
    public abstract class MemoryOperation : IDisposable
    {
        private bool m_disposed = false;

        protected string m_name;

        /// <summary>
        ///     Initializes a new instance of the MemoryOperation class, generates unique class dependant Name
        /// </summary>
        public MemoryOperation()
        {
            m_name = UniqueNameGenerator.GetUniqueObjectName(this);
        }

        /// <summary>
        ///     Returns true if this MemoryOperation is currently applied.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        ///     Returns true if this MemoryOperation is currently applied.
        /// </summary>
        public virtual bool IsApplied { get; protected set; }

        /// <summary>
        ///     Applies this MemoryOperation to memory. (Writes new bytes to memory)
        /// </summary>
        public abstract bool Apply();

        #region IDisposablePattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MemoryOperation()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool _disposing)
        {
            // just a copy&paste template of overriden Dispose(bool disposing) func
            if (!m_disposed)
            {
                if (_disposing)
                {
                    // Dispose managed resources.  
                }
                // Dispose unmanaged resources.
                // Note disposing has been done.
                m_disposed = true;
            }
        }

        #endregion
    }
}