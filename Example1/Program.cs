using System;
using System.Collections.Generic;
using AMQ.Wrapper;
using Apache.NMS;

namespace Example1
{
    class Program
    {
        static void Main(string[] args)
        {
            var queueHandler = new DefaultQueueHandler("activemq:tcp://localhost:61616", "amq.test1",
                new List<IMessageHandler>() { new SampleMessageHandler() });

            queueHandler.QueueHandlingErrorListener = QueueHandlingErrorListener;

            queueHandler.StartSession();

            Console.ReadKey();

            queueHandler.Dispose();
        }

        private static void QueueHandlingErrorListener(Exception exception, IMessage message, bool handled, bool acknowledged)
        {
            Console.WriteLine("ERROR | error occurred while processing message '{0}' Error: {1}", message.NMSCorrelationID, exception.Message);
        }
    }
}
