using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.Util;

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
                var connecturi = new Uri(uri);
                Console.WriteLine("About to connect to " + connecturi);

                // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
                IConnectionFactory factory = new NMSConnectionFactory(connecturi);

                using (var connection = factory.CreateConnection())
                using (var session = connection.CreateSession())
                {
                    var destination = SessionUtil.GetDestination(session, path);
                    Console.WriteLine("Using destination: " + destination);

                    // Create a producer
                    using (var producer = session.CreateProducer(destination))
                    {
                        // Start the connection so that messages will be processed.
                        connection.Start();
                        producer.DeliveryMode = MsgDeliveryMode.Persistent;

                        // Send a message
                        var message = session.CreateTextMessage(DateTime.Now.ToString("T") + ": Hello World!");
                        message.NMSCorrelationID = Guid.NewGuid().ToString();
                        message.Properties["NMSXGroupID"] = "Group-" + Guid.NewGuid();
                        message.Properties["myHeader"] = "Header-" + DateTime.Now.ToString("T");
                        message.Properties["counter"] = count;
                        producer.Send(message);
                        count++;
                    }
                }
                Thread.Sleep(5000);
            }
        }
    }
}