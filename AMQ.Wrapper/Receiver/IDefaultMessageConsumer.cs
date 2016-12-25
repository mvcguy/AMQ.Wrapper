using Apache.NMS;

namespace AMQ.Wrapper.Receiver
{
    public interface IDefaultMessageConsumer : IMessageConsumer
    {
        string QueueUri { get; }

        string QueuePath { get; }

        QueueSettings QueueSettings { get; }

        ISession Session { get; }

        IConnection Connection { get; }

        IDestination Destination { get; }

        IConnectionFactory ConnectionFactory { get; }

        /// <summary>
        /// Stops all Message delivery in this session and restarts it again
        /// with the oldest unabcknowledged message.  Messages that were delivered
        /// but not acknowledge should have their redelivered property set.
        /// This is an optional method that may not by implemented by all NMS
        /// providers, if not implemented an Exception will be thrown.
        /// Message redelivery is not requried to be performed in the original
        /// order.  It is not valid to call this method on a Transacted Session.
        /// </summary>
        void ResetSession();

        /// <summary>
        /// closes the connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// This will start the consumer to consume messages from the queue.
        /// The queue settings are optional
        /// By default the session is initialized with AcknowledgementMode=AcknowledgementMode.IndividualAcknowledge
        /// but you can change this behaviour using the settings
        /// </summary>
        void StartConsumer();

        event ExceptionListener ConnectionExceptionListener;
    }
}