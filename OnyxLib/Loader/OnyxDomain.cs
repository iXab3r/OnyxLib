#region Usings

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using XMLib;
using XMLib.Log;

#endregion

namespace OnyxLib.Loader
{
    /// <summary>
    ///     The actual domain object we'll be using to load and run the Onyx binaries.
    /// </summary>
    internal class OnyxDomain 
    {
        private readonly string m_assemblyPath;

        private AppDomain m_assemblyDomain;

        private Assembly m_onyxLib = Assembly.GetExecutingAssembly();

        private ProxyDomain m_proxyDomain;

        /// <summary>
        ///   Created AppDomain and loads target Assembly into it
        /// </summary>
        /// <param name="_assemblyPath"></param>
        public OnyxDomain(string _assemblyPath)
        {
	        if (_assemblyPath == null)
	        {
		        throw new ArgumentNullException(nameof(_assemblyPath));
	        }
	        m_assemblyPath = _assemblyPath;
            var assemblyBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Logger.DebugFormat("[OnyxDomain..ctor] Creating new AppDomain, assemblyName - {0}, base - {1}...", _assemblyPath, assemblyBase);
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = assemblyBase,
            };
            m_assemblyDomain = AppDomain.CreateDomain(string.Format("OnyxDomain_{0}", Guid.NewGuid()), null, appDomainSetup);
            Logger.DebugFormat("[OnyxDomain..ctor] AppDomain.FriendlyName = '{0}'", m_assemblyDomain.FriendlyName);

            var assemblyResolverTypeName = typeof(AssemblyResolver).FullName;
            Logger.DebugFormat("[OnyxDomain..ctor] Loading {0}(assembly {1}) into {2}", assemblyResolverTypeName, m_onyxLib.FullName, m_assemblyDomain.FriendlyName);
            // загружаем OnyxLib + AssemblyResolver в целевой домен
            m_assemblyDomain.CreateInstanceAndUnwrap(m_onyxLib.FullName, assemblyResolverTypeName);
            // далее грузим ProxyDomain, который будет принимать команды на загрузку/выгрузку сборки
            var proxyDomainTypeName = typeof(ProxyDomain).FullName;
            Logger.DebugFormat("[OnyxDomain..ctor] Loading {0}(assembly {1}) into {2}", proxyDomainTypeName, m_onyxLib.FullName, m_assemblyDomain.FriendlyName);
            // загружаем целевую сборку в ProxyDomain
            m_proxyDomain = (ProxyDomain)m_assemblyDomain.CreateInstanceAndUnwrap(
                m_onyxLib.FullName, 
                proxyDomainTypeName, 
                false, 
                BindingFlags.CreateInstance, 
                null, 
                new object[] { _assemblyPath }, 
                null, 
                null);
            Logger.DebugFormat("[OnyxDomain..ctor] Successfully loaded assembly {0} in AD {1}", _assemblyPath, m_assemblyDomain.FriendlyName);
        }

        /// <summary>
        ///    Unloads domain and target assembly
        /// </summary>
        public void Unload()
        {
            Logger.DebugFormat("[OnyxDomain.Unload] Unloading " + m_assemblyDomain.FriendlyName);
            m_proxyDomain.Dispose();
            AppDomain.Unload(m_assemblyDomain);
            Logger.DebugFormat("[OnyxDomain.Unload] Unloaded successfully");
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return String.Format("[OnyxDomain] {0} ({1})", m_assemblyDomain.FriendlyName, m_assemblyPath);
        }
    }
}