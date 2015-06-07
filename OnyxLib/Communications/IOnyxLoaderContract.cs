#region Usings

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

#endregion

namespace OnyxLib.Communications
{
    [ServiceContract]
    public interface IOnyxLoaderContract : IOnyxJournal
    {
        /// <summary>
        ///   NOP
        /// </summary>
        [OperationContract]
        [WebGet]
        string Ping();

        /// <summary>
        ///   Loads target assembly into process
        /// </summary>
        /// <param name="_assemblyPath">Full path to .NET assembly</param>
        [OperationContract]
        [WebGet]
        void LoadAssembly(string _assemblyPath);

        /// <summary>
        ///   Unloads all assemblies from process, except for OnyxLib
        /// </summary>
        [OperationContract]
        [WebGet]
        void UnloadAllAssemblies();
    }

    [ServiceContract]
    public interface IOnyxJournal
    {
        /// <summary>
        ///   Returns bunch of journal entries
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IEnumerable<OnyxJournalEntry> QueryJournal();
    }
}