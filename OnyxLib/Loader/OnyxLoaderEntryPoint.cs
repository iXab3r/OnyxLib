#region Usings

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using OnyxLib.Communications;

#endregion

namespace OnyxLib.Loader
{
    /// <summary>
    ///   Entry point for OnyxLoader
    /// </summary>
    public class OnyxLoaderEntryPoint
    {
        private static Process m_process = Process.GetCurrentProcess();

        private static Assembly m_onyxLib = Assembly.GetExecutingAssembly();


        private static OnyxServiceHost<IOnyxLoaderContract> m_host;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int Main(string _args)
        {
            try
            {
                EventLogLogger.Write(EventLogEntryType.Information, "OnyxLoader loaded successfully into {0} #{1}", m_process, m_process.Id);
                InitializeLoggingSubsystem();
                InitializeWcfHost();
                EnterEndlessCycle();
                return 0;
            }
            catch (Exception ex)
            {
                EventLogLogger.Write(EventLogEntryType.Error, "Exception occurred during initialization: {0}\r\n{1}", ex, ex.StackTrace);
                return -1;
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void InitializeWcfHost()
        {
            var instance = new OnyxLoader();
            EventLogLogger.Write(EventLogEntryType.Information, "Initializing wcf host of type {0} (instance {1})", typeof(IOnyxLoaderContract), instance);
            m_host = new OnyxServiceHost<IOnyxLoaderContract>(instance);
            m_host.Start();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void EnterEndlessCycle()
        {
            EventLogLogger.Write(EventLogEntryType.Information, "Onyx successfully initialized, awaiting for commands @ {0}...", OnyxServiceHost<IOnyxLoaderContract>.GeneratePipeUri(m_process.Id));
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Если не использовать подобный цикл, то при выгрузке последнего домена целевая программа вылетает
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            while (true)
            {
                Thread.Sleep(100);
            }
        }

       


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void InitializeLoggingSubsystem()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var exeName = Path.GetFileNameWithoutExtension(m_process.MainModule.FileName);
                var logDirectoryName = Path.Combine(appData, "OnyxLib", "Logs", exeName);
                var logName = String.Format("[{0}] OnyxLoader {1}.log", m_process.Id, DateTime.Now.ToString("yy-MM-dd HH-mm-ss"));
                var logPath = Path.Combine(logDirectoryName, logName);
                EventLogLogger.Write(EventLogEntryType.Information, "OnyxLoader Write path - {0}", logPath);

                XMLib.Log.Logger.InitializeLocalLogger(XMLib.Log.Logger.NativeLogger.Logger.Repository, log4net.Core.Level.All, logPath);
				XMLib.Log.Logger.InfoFormat("Logger initialized successfully");
				XMLib.Log.Logger.DumpApplicationInfo();
				XMLib.Log.Logger.InfoFormat("OnyxLib - {0} @ {1}", m_onyxLib.FullName, m_onyxLib.Location);
            }
            catch (Exception ex)
            {
				XMLib.Log.Logger.Error(ex);
            }
        }
    }

}