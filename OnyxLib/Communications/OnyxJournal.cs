using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using XMLib;

namespace OnyxLib.Communications
{
    public class OnyxJournal : IOnyxJournal
    {
        private ConcurrentQueue<OnyxJournalEntry> m_queuedJournalEntries = new ConcurrentQueue<OnyxJournalEntry>();

        public void Ping()
        {
        }
        
        public void AddJournalEntry(OnyxJournalEntry _entry)
        {
	        if (_entry == null)
	        {
		        throw new ArgumentNullException(nameof(_entry));
	        }
	        m_queuedJournalEntries.Enqueue(_entry);
        }

	    public void AddJournalEntry(string _text, params object[] _args)
        {
            m_queuedJournalEntries.Enqueue(new OnyxJournalEntry(String.Format(_text, _args), null));
        }

        /// <summary>
        ///   Returns bunch of journal entries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OnyxJournalEntry> QueryJournal()
        {
            var result = new List<OnyxJournalEntry>();
            var maxItemsCount = m_queuedJournalEntries.Count;

            OnyxJournalEntry entry;
            while (result.Count < maxItemsCount && m_queuedJournalEntries.TryDequeue(out entry))
            {
                result.Add(entry);
            }
            return result;
        }
    }
}