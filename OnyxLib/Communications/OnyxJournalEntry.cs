#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace OnyxLib.Communications
{
    [DataContract]
    public class OnyxJournalEntry
    {
        [DataMember]
        public object LinkedObject;

        [DataMember]
        public string Text;

        [DataMember]
        public DateTime TimeStamp;

        public OnyxJournalEntry(string newText, object newLinkedObject)
        {
            Text = newText;
            LinkedObject = newLinkedObject;
            TimeStamp = DateTime.Now;
        }
    }
}