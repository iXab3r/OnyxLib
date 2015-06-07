using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Instrumentation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

using log4net.Core;

using XMLib.Log;

using OnyxLib.Communications;

namespace OnyxLib.Loader
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class OnyxLoader : IOnyxLoaderContract
    {
        private LoadedDomainsList m_domainsList = new LoadedDomainsList();

        private OnyxJournal m_journal = new OnyxJournal();

        private static Assembly m_onyxLib = Assembly.GetExecutingAssembly();

        public OnyxLoader()
        {
        }

        /// <summary>
        ///   NOP
        /// </summary>
        public string Ping()
        {
            return DateTime.Now.ToString("F");
        }

        /// <summary>
        ///   Unloads all assemblies from process, except for OnyxLib
        /// </summary>
        public void UnloadAllAssemblies()
        {
            try
            {
                // currently multiple domains are not supported
                Logger.InfoFormat("[Wcf, UnloadAllAssemblies] Unloading all domains...");
                Logger.InfoFormat("[Wcf, UnloadAllAssemblies] Current domains list({0}):\r\n\t{1}", m_domainsList.Count, String.Join("\r\n\t", m_domainsList));
                m_domainsList.UnloadAll();
                Logger.InfoFormat("[Wcf, UnloadAllAssemblies] Successfully unloaded all domains");
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Cokuld noad perform .UnloadAllAssemblies"), ex);
                throw;
            }
        }


        /// <summary>
        ///   Returns bunch of journal entries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OnyxJournalEntry> QueryJournal()
        {
            try
            {
                return m_journal.QueryJournal();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Could noad perform .QueryJournal()"), ex);
                throw;
            }
        }

        /// <summary>
        ///   Loads target assembly into process
        /// </summary>
        /// <param name="_assemblyPath">Full path to .NET assembly</param>
        public void LoadAssembly(string _assemblyPath)
        {
            try
            {
                // currently multiple domains are not supported
                Logger.InfoFormat("[Wcf, Load assembly] Loading assembly from '{0}'", _assemblyPath);
                Logger.InfoFormat("[Wcf, Load assembly] Current domains list({0}):\r\n\t{1}", m_domainsList.Count, String.Join("\r\n\t", m_domainsList));
                Logger.InfoFormat("[Wcf, Load assembly] Unloading all domains...", m_domainsList.Count, String.Join("\r\n\t", m_domainsList));
                m_domainsList.UnloadAll();
                var domain = new OnyxDomain(_assemblyPath);
                m_domainsList.Add(domain);
                Logger.InfoFormat("[Wcf, Load assembly] Successfully loaded '{0}' into {1}", _assemblyPath, domain);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Could noad perform .LoadAssembly('{0}')", _assemblyPath), ex);
                throw;
            }
        }



        /// <summary>
        ///   Injects loader into target process. After that you can access OnyxLoader via WCF
        /// </summary>
        /// <param name="_targetProcess"></param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InjectLoaderIntoProcess(Process _targetProcess)
        {
            if (_targetProcess == null)
            {
                throw new ArgumentNullException("_targetProcess");
            }

            if (TryToConnectToOnyxLoader(_targetProcess, 1))
            {
                Logger.DebugFormat("OnyxLoader already loaded into process {0} #{1}", _targetProcess.ProcessName, _targetProcess.Id);
            }
            else
            {
                Logger.DebugFormat("[InjectLoaderIntoProcess] Injecting Onyx into {0} #{1}...", _targetProcess.ProcessName, _targetProcess.Id);
                var onyxLibName = Path.GetFileName(m_onyxLib.Location);
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var duplicateLibPath = Path.Combine(appData, "OnyxLib", "Temp", m_onyxLib.GetName().Version+"_"+Guid.NewGuid().ToString().Replace(@"-", String.Empty), onyxLibName);
                Logger.DebugFormat("[InjectLoaderIntoProcess] Creating OnyxLib duplicate @'{0}'", duplicateLibPath);
                Directory.CreateDirectory(Path.GetDirectoryName(duplicateLibPath));
                File.Copy(m_onyxLib.Location, duplicateLibPath);


                Logger.DebugFormat("[InjectLoaderIntoProcess] Injecting Onyx into {0} #{1} from '{2}'...", _targetProcess.ProcessName, _targetProcess.Id, duplicateLibPath);
                OnyxHelpers.InjectAndExecuteInDefaultAppDomain(_targetProcess.Id, duplicateLibPath, typeof(OnyxLoaderEntryPoint).ToString(), "Main", String.Empty);

                Logger.DebugFormat("[InjectLoaderIntoProcess] Awaiting for connection to WCF host in {0} #{1}...", _targetProcess.ProcessName, _targetProcess.Id);
                if (!TryToConnectToOnyxLoader(_targetProcess))
                {
                    throw new ApplicationException(string.Format("Could not inject OnyxLoader into process {0} #{1}", _targetProcess.ProcessName, _targetProcess.Id));
                }
                Logger.DebugFormat("[InjectLoaderIntoProcess] Successfully injected OnyxLoader into process {0} #{1}", _targetProcess.ProcessName, _targetProcess.Id);
            }
        }

        /// <summary>
        ///   Tests connection to target process via WCF
        /// </summary>
        /// <param name="_targetProcess"></param>
        /// <param name="_maxAttemptsCount"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool TryToConnectToOnyxLoader(Process _targetProcess, int _maxAttemptsCount = 100)
        {
            if (_targetProcess == null)
            {
                throw new ArgumentNullException("_targetProcess");
            }
            var binding = OnyxServiceHost<IOnyxLoaderContract>.GeneratePipeBinding();
            var serviceName = OnyxServiceHost<IOnyxLoaderContract>.GeneratePipeUri(_targetProcess.Id);
            var client = ChannelFactory<IOnyxLoaderContract>.CreateChannel(binding, new EndpointAddress(serviceName));

            var attemptsCount = 0;
            var connectedSuccessfully = false;
            while (attemptsCount < _maxAttemptsCount)
            {
                try
                {
                    client.Ping();
                    connectedSuccessfully = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
                attemptsCount++;
            }
            return connectedSuccessfully;
        }
    }
}