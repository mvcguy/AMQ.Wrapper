using System;
using AMQ.Wrapper.Receiver;
using Apache.NMS;

namespace Example2
{
    public class DefaultMessageHandler : IMessageHandler
    {
        private readonly IMessageStorageService _storageService;


        public DefaultMessageHandler(IMessageStorageService storageService = null)
        {
            _storageService = storageService ?? new MessageStorageService();
            HandlerName = "DefaultMessageHandler";
        }

        public bool? HandleMessage(IMessage message)
        {
            //just a sample
            var historyItem = _storageService.GetMessageHistoryItem(message.NMSCorrelationID);

            if (historyItem != null && historyItem.Redelivered) return null;

            bool handled;

            try
            {
                Console.WriteLine("DefaultMessageHandler: Message Received: {0}", (message as ITextMessage)?.Text);
                handled = true;
            }
            catch (Exception)
            {
                handled = false;
            }

            _storageService.AddOrUpdateMessageHistoryItem(message, handled);
            return handled;
        }

        public string HandlerName { get; }
    }
}