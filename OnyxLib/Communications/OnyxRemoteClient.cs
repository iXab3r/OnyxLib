using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace OnyxLib.Communications
{
    public class OnyxRemoteClient : ClientBase<IOnyxLoaderContract>, IOnyxLoaderContract
    {
        public OnyxRemoteClient(int _processId)
            : base(OnyxServiceHost<IOnyxLoaderContract>.GeneratePipeBinding(), new EndpointAddress(OnyxServiceHost<IOnyxLoaderContract>.GeneratePipeUri(_processId)))
        {

        }

        /// <summary>
        ///   Returns bunch of journal entries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OnyxJournalEntry> QueryJournal()
        {
            return base.Channel.QueryJournal();
        }

        /// <summary>
        ///   NOP
        /// </summary>
        public string Ping()
        {
            return base.Channel.Ping();
        }

        /// <summary>
        ///   Loads target assembly into process
        /// </summary>
        /// <param name="_assemblyPath">Full path to .NET assembly</param>
        public void LoadAssembly(string _assemblyPath)
        {
            base.Channel.LoadAssembly(_assemblyPath);
        }

        /// <summary>
        ///   Unloads all assemblies from process, except for OnyxLib
        /// </summary>
        public void UnloadAllAssemblies()
        {
            base.Channel.UnloadAllAssemblies();
        }
    }
}
