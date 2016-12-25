using System;
using System.Collections.Generic;
using System.Threading;
using AMQ.Wrapper.Receiver;
using Apache.NMS;

namespace Example2
{
    public class Program
    {
        static void Main(string[] args)
        {
            
            IQueueHandler queueHandler = new DefaultQueueHandler(
                AppConstants.QueueUri,
                AppConstants.QueuePath,
                new List<IMessageHandler>()
                {
                    new DefaultMessageHandler()
                }, new SessionResetPolicy()
                {
                    ShouldResetSession = true,
                    ResetFrequencyHours = 12
                });

            queueHandler.AdvancedSettings.ConfigSessionFunc = connection =>
            {
                var session = connection.CreateSession(AcknowledgementMode.Transactional);
                return session;
            };

            queueHandler.AdvancedSettings.ConfigConnectionFunc = factory =>
            {
                var connection = factory.CreateConnection();
                connection.RedeliveryPolicy.MaximumRedeliveries = 4;
                return connection;
            };

            queueHandler.ConnectionExceptionListener += QueueHandler_ConnectionExceptionListener;

            queueHandler.AdvancedSettings.ConfigAckBehaviour = (message, handled, session) =>
            {
                session.Commit();
                return true;
                //if the queue is AcknowledgementMode.IndividualAcknowledge then do like this:
                //message.Acknowledge();
                //return true;

            };

            queueHandler.AdvancedSettings.ConfigInternalLogBehaviour = (message, level) =>
            {
                Console.WriteLine(message);
            };

            queueHandler.StartSession();

            var thread = new Thread(() =>
            {
                SampleMessageProducer.SimulateMessageSending(AppConstants.QueueUri, AppConstants.QueuePath);
            });

            thread.Start();

            Console.ReadLine();
            queueHandler.Dispose();
        }

        private static void QueueHandler_ConnectionExceptionListener(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }


}
