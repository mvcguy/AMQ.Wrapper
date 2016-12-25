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
                item.MessageProcessed = true;
                item.LastProcessedOn = DateTime.Now;
            }
            else
            {
                item = new MessageHistoryItem()
                {
                    CreatedDatetime = DateTime.Now,
                    LastProcessedOn = DateTime.Now,
                    MessageProcessed = false,
                    MessageId = new Guid(message.NMSCorrelationID)
                };
                Store.Add(item);
            }
            return item;
        }
    }
}