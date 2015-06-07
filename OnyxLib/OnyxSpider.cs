using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using XMLib;
using XMLib.Log;


namespace OnyxLib
{
    public class OnyxSpider
    {
        private readonly Regex m_processRegex;

        private Task m_task;

        private CancellationTokenSource m_cancellationToken;

        private TimeSpan m_recheckPeriod = TimeSpan.FromMilliseconds(500);

        private HashSet<int> m_processedProcesses = new HashSet<int>();

	    public OnyxSpider()
		    : this(null)
	    {
	    }

	    public OnyxSpider(Regex _processRegex)
        {
            if (_processRegex == null)
            {
                throw new ArgumentNullException("_processRegex");
            }

            m_processRegex = _processRegex;
        }

        /// <summary>
        ///   Период сканирования процессов
        /// </summary>
        public TimeSpan RecheckPeriod
        {
            get
            {
                return m_recheckPeriod;
            }
            set
            {
                m_recheckPeriod = value;
            }
        }

        public void Start()
        {
            Logger.DebugFormat("[OnyxSpider.Start] Starting thread...");
            if (m_task != null)
            {
                throw new InvalidOperationException("Task is already created");
            }
            m_cancellationToken = new CancellationTokenSource();
            m_task = new Task(WorkerThread, m_cancellationToken.Token, TaskCreationOptions.LongRunning);
            m_task.Start();
            Logger.DebugFormat("[OnyxSpider.Start] Thread started");
        }

        public void Stop()
        {
            Logger.DebugFormat("[OnyxSpider.Stop] Stopping thread...");
            if (m_task != null)
            {
                m_cancellationToken.Cancel();
                if (!m_task.Wait(m_recheckPeriod.Add(m_recheckPeriod)))
                {
                    throw new ApplicationException("Could not stop task execution");
                }
                m_task = null;
                m_cancellationToken = null;
            }
            Logger.DebugFormat("[OnyxSpider.Start] Thread stopped");
        }

        private void WorkerThread()
        {
            Logger.DebugFormat("{0}",ThreadUtils.DumpThreadInfo());
            while (!m_cancellationToken.Token.WaitHandle.WaitOne(m_recheckPeriod))
            {
                try
                {
                    var processesList = Process.GetProcesses();
                    var matchingProcesses = processesList.Where(x => m_processRegex.IsMatch(x.ProcessName)).ToArray();

                    var processToInject = matchingProcesses.Where(x=>!m_processedProcesses.Contains(x.Id)).ToArray();

                    if (processToInject.Any())
                    {
                        Logger.DebugFormat("Found {0} target process(es):\r\n\t{1}",
                            processToInject.Length,
                            String.Join("\r\n\t",processToInject.Select(x=>String.Format("[{0}] {1}, mainModule {2}",x.Id, x.ProcessName, x.MainModule))));
                        foreach (var process in processToInject)
                        {
                            ProcessFound(this, new EventArgs<Process>(process));
                            m_processedProcesses.Add(process.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception occured in task cycle",ex);
                }
            }
        }

        public event EventHandler<EventArgs<Process>> ProcessFound = delegate { };
    }
}