#region Usings

using System;
using System.IO;
using System.Reflection;

#endregion

namespace FasmWrapper
{
    internal static class ResourcesHelper
    {
        private static Assembly m_thisAssembly = Assembly.GetExecutingAssembly();

        static ResourcesHelper()
        {
        }

        /// <summary>
        ///     Loads resource bytes by name
        /// </summary>
        /// <param name="_resourceName"></param>
        /// <returns></returns>
        public static byte[] GetResourcesBytes(string _resourceName)
        {
            if (String.IsNullOrEmpty(_resourceName))
            {
                throw new ArgumentNullException("_resourceName");
            }

            var assemblyNamespace = typeof(ResourcesHelper).Namespace;

            if (assemblyNamespace != null && !_resourceName.StartsWith(assemblyNamespace + "."))
            {
                _resourceName = String.Format("{0}.{1}", assemblyNamespace, _resourceName);
            }

            var resources = m_thisAssembly.GetManifestResourceNames();
            using (var resourceStream = m_thisAssembly.GetManifestResourceStream(_resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ApplicationException(
                        string.Format(
                            "Could not get resources '{0}'. Resources list({1}):\r\n\t{2}",
                            _resourceName,
                            resources.Length,
                            String.Join("\r\n\t", resources)));
                }
                return new BinaryReader(resourceStream).ReadBytes((int)resourceStream.Length);
            }
        }
    }
}