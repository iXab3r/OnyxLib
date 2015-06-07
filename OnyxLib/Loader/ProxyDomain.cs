using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using XMLib.Log;

namespace OnyxLib.Loader
{
    internal class ProxyDomain : MarshalByRefObject, IDisposable
    {
        private readonly string m_assemblyPath;

        private IOnyxEntryPoint m_onyxEntryPointImplementation;

        private Assembly m_loadedAssembly;
        private Process m_process = Process.GetCurrentProcess();

        public ProxyDomain(string _assemblyPath)
        {
            m_assemblyPath = _assemblyPath;
            InitializaLogging();
            LoadAssembly(_assemblyPath);
        }

        private void InitializaLogging()
        {
            var logFileName = String.Format("[{0}] {1}.log", m_process.Id, Path.GetFileNameWithoutExtension(m_assemblyPath));
            var logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OnyxLib", "logs", m_process.ProcessName, logFileName);
            Logger.InitializeLocalLogger(logFilePath);
        }

        /// <summary>
        ///    Loads target assembly into current AppDomain
        /// </summary>
        /// <param name="_assemblyPath"></param>
        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void LoadAssembly(string _assemblyPath)
        {
            if (m_loadedAssembly != null)
            {
                throw new InvalidOperationException(String.Format("Assembly '{0}' already loaded in this domain, you cannot load any more assemblies", m_loadedAssembly));
            }
            Logger.DebugFormat("[AR.LoadAssembly] Loading {0} in AD '{1}'", _assemblyPath, AppDomain.CurrentDomain.FriendlyName);
            m_loadedAssembly = Assembly.LoadFrom(_assemblyPath);
            if (_assemblyPath == null)
            {
                throw new ArgumentNullException("_assemblyPath");
            }
            var entryPointType = m_loadedAssembly.GetTypes().FirstOrDefault(x => typeof(IOnyxEntryPoint).IsAssignableFrom(x));
            if (entryPointType == null)
            {
                throw new ApplicationException(String.Format("Could not find class, that implements {0} in {1}", typeof(IOnyxEntryPoint), m_loadedAssembly));
            }
            Logger.DebugFormat("[AR.LoadAssembly] Instantiating {1}.'{0}' in AD '{2}'", entryPointType.FullName, m_loadedAssembly, AppDomain.CurrentDomain.FriendlyName);
            m_onyxEntryPointImplementation = (IOnyxEntryPoint)Activator.CreateInstance(entryPointType);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (m_onyxEntryPointImplementation == null)
            {
                return;
            }
            Logger.DebugFormat("[AR.UnloadAssembly] Disposing instance {0}...", m_onyxEntryPointImplementation);
            try
            {
                m_onyxEntryPointImplementation.Dispose();
                Logger.DebugFormat("[AR.UnloadAssembly] Successfully disposed", m_onyxEntryPointImplementation);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("[AR.UnloadAssembly] Exception occurred during disposing {0} - {1}\r\n{2}", m_onyxEntryPointImplementation, ex.Message, ex.StackTrace);
            }
            m_onyxEntryPointImplementation = null;
        }
    }
}
