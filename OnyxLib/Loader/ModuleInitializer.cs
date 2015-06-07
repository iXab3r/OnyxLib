// This file can be modified in any way, with two exceptions. 1) The name of
// this class must be "ModuleInitializer". 2) This class must have a public or
// internal parameterless "Run" method that returns void. In addition to these
// modifications, this file may also be moved to any location, as long as it
// remains a part of its current project.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using OnyxLib.Loader;

namespace OnyxLib
{
    internal static class ModuleInitializer
    {
        private static AssemblyResolver m_assemblyResolver;

        internal static void Run()
        {
            EventLogLogger.Write(EventLogEntryType.Information, "Module initializer started");
            m_assemblyResolver = new AssemblyResolver();
        }
    }
}