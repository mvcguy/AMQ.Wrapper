using System;
using AMQ.Wrapper.Receiver;
using Apache.NMS;

namespace Example1
{
    public class SampleMessageHandler : IMessageHandler
    {
        public SampleMessageHandler()
        {
            HandlerName = "Sample Message Handler";
        }

        public bool? HandleMessage(IMessage message)
        {
            var handled = false;

            //handle the message and return true if successfull, otherwise return false
            // if returned true, the message will be acknowledged
            //otherwise it will be stay in the queue.
            Console.WriteLine("Message '{0}' is handled", message.NMSCorrelationID);

            handled = true;

            return handled;

        }

        public string HandlerName { get; private set; }
    }
}