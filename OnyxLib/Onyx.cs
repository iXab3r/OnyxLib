#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using XMLib;
using XMLib.Log;

using OnyxLib.Communications;
using OnyxLib.Loader;
using OnyxLib.Managers;
using OnyxLib.MemoryOperations;

#endregion

namespace OnyxLib
{
    public class Onyx : LazySingleton<Onyx>, IDisposable
    {
        private readonly Process m_targetProcess;

        /// <summary>
        ///     Retrieves an instance of the <see cref="DetourManager" /> class for opened process
        /// </summary>
        public DetourManager Detours
        {
            get
            {
                return m_detours;
            }
        }

        /// <summary>
        ///     Retrieves an instance of the <see cref="OnyxMemory" /> class for opened process
        /// </summary>
        public OnyxMemory Memory
        {
            get
            {
                return m_memory;
            }
        }

        /// <summary>
        ///     Provides protections against deadlocks in detours, DISABLING IT WILL LEAD YOU INTO HELL
        /// </summary>
        private OnyxThreadInterceptor ThreadInterceptor;

        private DetourManager m_detours;

        private OnyxMemory m_memory;

        public Onyx()
            : this(Process.GetCurrentProcess())
        {
        }

        public Onyx(int _pid)
            : this(Process.GetProcessById(_pid))
        {
           
        }

        public Onyx(Process _targetProcess)
        {
	        if (_targetProcess == null)
	        {
		        throw new ArgumentNullException(nameof(_targetProcess));
	        }
	        m_targetProcess = _targetProcess;
            m_memory = new OnyxMemory(_targetProcess.Id);
            m_detours = new DetourManager();
        }

        /// <summary>
        ///   ProcessId of associated process
        /// </summary>
        public Process ProcessId { get; protected set; }


        public void InitializeThreadInterceptor()
        {
            if (IntPtr.Size == 4) // 32 bit
            {
                ThreadInterceptor = new OnyxThreadInterceptor();
                ThreadInterceptor.Apply();
            } else
            {
                throw new NotSupportedException("ThreadInterception is not supported in 64-bit apps");
            }
        }

        /// <summary>
        ///  Injects OnyxLoader into target process, allowing you to access it using WCF
        /// </summary>
        public OnyxRemoteClient InjectLoader()
        {
            OnyxLoader.InjectLoaderIntoProcess(m_targetProcess);
            var client = new OnyxRemoteClient(m_targetProcess.Id);
            return client;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (m_detours != null)
            {
                m_detours.RemoveAll();
            }
            if (m_memory != null)
            {
                m_memory.Dispose();
            }
        }
    }
}