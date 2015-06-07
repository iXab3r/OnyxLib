using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using OnyxLib.Loader;

namespace OnyxLib
{
    /// <summary>
    ///   Class, that hooks all neccessary events for handling binaries loading in AppDomains
    ///   One of the most important classes in the lib. You should not make changes if you're not sure of what are you doing.
    ///   For example, you MUST NOT use any external libraries in this class
    /// </summary>
    internal class AssemblyResolver : MarshalByRefObject
    {
        private Assembly m_onyxLib = Assembly.GetExecutingAssembly();

        private Process m_process = Process.GetCurrentProcess();

        private static readonly string m_resourcesDirectory = "Externals";

     
        public AssemblyResolver()
        {
            InitializeAssemblyResolver();
        }

        /// <summary>
        ///   Initializes .AssemblyResolve event in current AppDomain
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void InitializeAssemblyResolver()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                EventLogLogger.Write(
                    EventLogEntryType.Information,
                    "AppDomain: {3}\r\n\tOnyxLib: {4}\r\n\tAssemblies:\r\n\t{0}\r\n\r\nLoaded assemblies({1}):\r\n\t{2}",
                    String.Join(
                        "\r\n\t",
                        new[]
                    {
                        String.Format("{0} - {1}", "Executing", Assembly.GetExecutingAssembly()),
                        String.Format("{0} - {1}", "Calling", Assembly.GetCallingAssembly()), String.Format("{0} - {1}", "Entry", Assembly.GetEntryAssembly()),
                    }),
                    assemblies.Length,
                    String.Join<Assembly>("\r\n\t", assemblies),
                    AppDomain.CurrentDomain.FriendlyName,
                    String.Format("{0} @ {1}", m_onyxLib, m_onyxLib.Location)
                    );
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

                var resources = m_onyxLib.GetManifestResourceNames();
                // preload assemblies
                var embeddedAssembliesRegex = new Regex(@"\.(.*?.dll)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                EventLogLogger.Write(EventLogEntryType.Information, "[{0}] Resources({1}):\r\n\t{2}", AppDomain.CurrentDomain.FriendlyName, resources.Length, String.Join("\r\n\t", resources));
                var embeddedAssemblies = resources.Select(
                    x =>
                    {
                        var match = embeddedAssembliesRegex.Match(x);
                        if (match.Success)
                        {
                            return match.Groups[1].Value;
                        }
                        else
                        {
                            return null;
                        }
                    }).Where(x => x != null).ToArray();
                EventLogLogger.Write(EventLogEntryType.Information, "[{3}] Embedded assemblies({1}):\r\n\tRegex - {2}\r\n\t{0}",
                    String.Join("\r\n\t", embeddedAssemblies), embeddedAssemblies.Length, embeddedAssembliesRegex, AppDomain.CurrentDomain.FriendlyName);

                foreach (var embeddedAssembly in embeddedAssemblies)
                {
                    TryToLoadFromResources(embeddedAssembly);
                }

                assemblies = AppDomain.CurrentDomain.GetAssemblies();
                EventLogLogger.Write(EventLogEntryType.Information, "[{0}] Assemblies list after preload({1}):\r\n\t{2}",
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies.Length,
                    String.Join("\r\n\t", assemblies.Select(x => String.Format("{0} @ {1}", x, x.Location)))
                    );
            }
            catch (Exception ex)
            {
                EventLogLogger.Write(EventLogEntryType.Error, "[AD '{0}'] Exception during initialization - {1}\r\n{2}",
                    AppDomain.CurrentDomain.FriendlyName,
                    ex.Message,
                    ex.StackTrace);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private Assembly CurrentDomainOnAssemblyResolve(object _sender, ResolveEventArgs _args)
        {
            // сначала проверяем наличие необходимой библиотеки рядом с Onyx'ом
            if (_sender == null || _args == null)
            {
                return null;
            }
            try
            {
                //Use the AssemblyName class to get the name
                var assemblyName = String.Format("{0}.dll", new AssemblyName(_args.Name).Name);
                var result = TryToLoadFromResources(assemblyName);
                result = result ?? TryToLoadFromLocalPath(assemblyName);
                if (result == null)
                {
                    EventLogLogger.Write(EventLogEntryType.Warning, "[AD '{1}'] Could not find assembly '{0}'", assemblyName, AppDomain.CurrentDomain.FriendlyName);
                }
                return result;
            }
            catch (Exception ex)
            {
                EventLogLogger.Write(EventLogEntryType.Error, "[AD '{1}'] Exception during loading assembly '{0}' - {2}\r\n{3}",
                    _args.Name,
                    AppDomain.CurrentDomain.FriendlyName,
                    ex.Message,
                    ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        ///   Attempts to load assembly with specified name from local Onyx directories
        /// </summary>
        /// <param name="_assemblyName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private Assembly TryToLoadFromLocalPath(string _assemblyName)
        {
            var possibleLocationNearOnyx = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _assemblyName);
            if (File.Exists(possibleLocationNearOnyx))
            {
                EventLogLogger.Write(EventLogEntryType.Information, "[AD '{2}'] Loading assembly '{0}' from Onyx path '{1}'", _assemblyName, possibleLocationNearOnyx, AppDomain.CurrentDomain.FriendlyName);
                return Assembly.LoadFrom(possibleLocationNearOnyx);
            }
            return null;
        }

        /// <summary>
        ///   Attempts to load assembly with specified name from Onyx resources
        /// </summary>
        /// <param name="_assemblyName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private Assembly TryToLoadFromResources(string _assemblyName)
        {
            if (_assemblyName == null)
            {
                throw new ArgumentNullException("_assemblyName");
            }
            var namespaceName = typeof(AssemblyResolver).Namespace;
            var resourceName = String.Format("{0}.{1}.{2}", namespaceName, m_resourcesDirectory, _assemblyName);

            var assemblyNameWithoutExtension = Path.GetFileNameWithoutExtension(_assemblyName);
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssembly = loadedAssemblies.FirstOrDefault(x => assemblyNameWithoutExtension.Equals(x.GetName().Name, StringComparison.InvariantCultureIgnoreCase));
            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }
            using (var stream = m_onyxLib.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    EventLogLogger.Write(EventLogEntryType.Information, "[AD '{1}']Loading assembly '{0}' from Onyx resources", resourceName, AppDomain.CurrentDomain.FriendlyName);
                    var assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }
            return null;
        }

    }
}