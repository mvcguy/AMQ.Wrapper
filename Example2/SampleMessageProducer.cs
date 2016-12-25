using System;
using System.Threading;
using AMQ.Wrapper;
using Apache.NMS;

namespace Example2
{
    public class SampleMessageProducer
    {
        public static void SimulateMessageSending(string uri, string path)
        {
            var endTime = DateTime.Now.AddSeconds(300);
            var count = 0;

            while (endTime > DateTime.Now)
            {
                Console.WriteLine("About to connect to " + uri);

                IDefaultMessageProducer producer = new DefaultMessageProducer(uri, path);

                producer.DeliveryMode = MsgDeliveryMode.Persistent;

                producer.StartProducer();

                // Send a message
                var message = producer.Session.CreateTextMessage(DateTime.Now.ToString("T") + ": Hello World!");
                message.NMSCorrelationID = Guid.NewGuid().ToString();
                message.Properties["NMSXGroupID"] = "Group-" + Guid.NewGuid();
                message.Properties["myHeader"] = "Header-" + DateTime.Now.ToString("T");
                message.Properties["counter"] = count;
                producer.Send(message);
                count++;

                Thread.Sleep(5000);
            }
        }
    }
}