#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#endregion

namespace OnyxLib
{
    /// <summary>
    ///   Low-performance unique Id generator
    /// </summary>
    internal static class UniqueNameGenerator
    {
        private static Dictionary<Type, UInt64> m_uniqueIdCounter;

        static UniqueNameGenerator()
        {
            m_uniqueIdCounter = new Dictionary<Type, ulong>();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetUniqueObjectName(object _obj)
        {
            var objectType = _obj.GetType();
            if (_obj == null)
            {
                throw new ArgumentNullException("_obj");
            }
            if (!m_uniqueIdCounter.ContainsKey(objectType))
            {
                m_uniqueIdCounter.Add(objectType, 1);
            }
            return objectType.ToString() + m_uniqueIdCounter[objectType]++;
        }
    }
}