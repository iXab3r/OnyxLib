namespace OnyxLib.MemoryOperations
{
    public abstract class MemoryOperationReversable : MemoryOperation
    {
        /// <summary>
        ///     Removes this MemoryOperation from memory. (Reverts the bytes back to their originals.)
        /// </summary>
        public abstract bool Remove();

        /// <summary>
        ///     Modified destructor, we make sure, that  original memory state is restored when object disposes
        /// </summary>
        /// <param name="_disposing"></param>
        protected override void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                if (IsApplied)
                {
                    Remove();
                }
            }
            base.Dispose(_disposing);
        }
    }
}