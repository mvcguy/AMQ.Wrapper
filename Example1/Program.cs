using System;
using System.Collections.Generic;
using AMQ.Wrapper.Receiver;

namespace Example1
{
    class Program
    {
        static void Main(string[] args)
        {
            var queueHandler = new DefaultQueueHandler("activemq:tcp://localhost:61616", "amq.test1",
                new List<IMessageHandler>() { new SampleMessageHandler() });

            queueHandler.StartSession();

            Console.ReadKey();

            queueHandler.Dispose();
        }
    }
}
