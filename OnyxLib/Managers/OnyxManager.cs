#region Usings

using System.Collections.Generic;

using OnyxLib.MemoryOperations;

#endregion

namespace OnyxLib.Managers
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class OnyxManager<T>
        where T : MemoryOperationReversable
    {
        /// <summary>
        /// </summary>
        protected Dictionary<string, T> Operations = new Dictionary<string, T>();

        internal OnyxManager()
        {
        }

        /// <summary>
        ///     Retrieves an IMemoryOperation by name.
        /// </summary>
        /// <param name="_name">The name given to the IMemoryOperation</param>
        /// <returns></returns>
        public virtual T this[string _name]
        {
            get
            {
                return Operations[_name];
            }
        }

        /// <summary>
        ///     Applies all the IMemoryOperations contained in this manager via their Apply() method.
        /// </summary>
        public virtual void ApplyAll()
        {
            foreach (var dictionary in Operations)
            {
                dictionary.Value.Apply();
            }
        }

        /// <summary>
        ///     Removes all the IMemoryOperations contained in this manager via their Remove() method.
        /// </summary>
        public virtual void RemoveAll()
        {
            foreach (var dictionary in Operations)
            {
                dictionary.Value.Remove();
            }
        }

        /// <summary>
        ///     Deletes all the IMemoryOperations contained in this manager.
        /// </summary>
        public virtual void DeleteAll()
        {
            foreach (var dictionary in Operations)
            {
                dictionary.Value.Dispose();
            }
            Operations.Clear();
        }

        /// <summary>
        ///     Deletes a specific IMemoryOperation contained in this manager, by name.
        /// </summary>
        /// <param name="name"></param>
        public virtual void Delete(string name)
        {
            if (Operations.ContainsKey(name))
            {
                Operations[name].Dispose();
                Operations.Remove(name);
            }
        }
    }
}