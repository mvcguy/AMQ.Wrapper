using System;
using Apache.NMS;

namespace AMQ.Wrapper
{
    public interface IQueueHandler : IDisposable
    {
        /// <summary>
        /// This will start a new queue session and it will trigger the Handle method of all the registered handlers.
        /// Once a session is started, repeated calls to this method will be ignored
        /// </summary>
        void StartSession();

        /// <summary>
        /// use this configure queue session, connection factory, connection, or queue destination
        /// </summary>
        QueueSettings AdvancedSettings { get;}

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        /// any necessary transformations on the received message before it is delivered.
        /// </summary>
        ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        /// <summary>
        /// An asynchronous listener which can be notified if an error occurs
        /// </summary>
        event ExceptionListener ConnectionExceptionListener;

        DateTime SessionStartedOn { get;}
    }
}