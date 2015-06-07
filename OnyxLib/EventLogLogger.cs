using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnyxLib
{
    /// <summary>
    ///   Helper class for writing messages into Windows EventLog
    /// </summary>
    internal static class EventLogLogger
    {
        private static string m_sourceName;
        private static Process m_process = Process.GetCurrentProcess();

        static EventLogLogger()
        {
            m_sourceName = String.Format("{0} #{1}", m_process.ProcessName, m_process.Id);
        }

        /// <summary>
        ///   Writes message to event log
        /// </summary>
        /// <param name="_entryType"></param>
        /// <param name="_msg"></param>
        /// <param name="_args"></param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Write(EventLogEntryType _entryType, string _msg, params object[] _args)
        {
            if (!EventLog.SourceExists(m_sourceName))
            {
                EventLog.CreateEventSource(m_sourceName, "OnyxLoader");
            }
            EventLog.WriteEntry(m_sourceName, String.Format(_msg, _args), _entryType);
        }
    }
}
