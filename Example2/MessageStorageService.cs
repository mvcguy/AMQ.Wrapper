using System;
using System.Collections.Generic;
using Apache.NMS;

namespace Example2
{
    public class MessageStorageService : IMessageStorageService
    {
        public List<MessageHistoryItem> Store = new List<MessageHistoryItem>();

        public MessageHistoryItem GetMessageHistoryItem(string messageId)
        {
            return Store.Find(x => x.MessageId.ToString() == messageId);
        }

        public MessageHistoryItem AddOrUpdateMessageHistoryItem(IMessage message, bool isProcessed)
        {
            var item = GetMessageHistoryItem(message.NMSCorrelationID);
            if (item != null)
            {
                item.MessageProcessed = isProcessed;
                item.LastProcessedOn = DateTime.Now;
                item.Redelivered = true;
                item.RedeliveredCount = item.RedeliveredCount + 1;
            }
            else
            {
                item = new MessageHistoryItem()
                {
                    CreatedDatetime = DateTime.Now,
                    LastProcessedOn = DateTime.Now,
                    MessageProcessed = isProcessed,
                    MessageId = new Guid(message.NMSCorrelationID),
                    Redelivered = false,
                    RedeliveredCount = 0,
                };
                Store.Add(item);
            }
            return item;
        }
    }
}