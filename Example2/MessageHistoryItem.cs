using System;

namespace Example2
{
    public class MessageHistoryItem
    {
        public Guid? MessageId { get; set; }

        public string MessageData { get; set; }

        public bool Redelivered { get; set; }

        public int RedeliveredCount { get; set; }
        
        public bool MessageProcessed { get; set; }

        public DateTime? LastProcessedOn { get; set; }

        public DateTime? CreatedDatetime { get; set; }
    }
}