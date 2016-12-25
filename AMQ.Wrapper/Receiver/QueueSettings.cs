using System;
using Apache.NMS;

namespace AMQ.Wrapper.Receiver
{
    public class QueueSettings
    {
        public Func<string, ISession, IDestination> ConfigDestinationFunc;

        public Func<IConnection, ISession> ConfigSessionFunc;

        /// <summary>
        /// Use this delegate to initialize the queue connection.
        /// Do not register any listener here. To register listener use <see cref="DefaultMessageConsumer.ConnectionExceptionListener"/>.
        /// Also do not start the connection. It will be taken care by <see cref="DefaultMessageConsumer.InitConnection"/>
        /// </summary>
        public Func<IConnectionFactory, IConnection> ConfigConnectionFunc;

        public Func<string, IConnectionFactory> ConfigFactoryFunc;

        /// <summary>
        /// the delegate is used to initialize the message consumer.
        /// do not register any listener here. To register listener use <see cref="DefaultMessageConsumer.Listener"/>
        /// or <see cref="DefaultMessageConsumer.ConsumerTransformer"/>
        /// </summary>
        public Func<ISession, IDestination, IMessageConsumer> ConfigMessageConsumer;

        /// <summary>
        /// Use this delegate to configure the acknowledgment behaviour. 
        /// This will be called after a message is handled by the handlers
        /// </summary>
        public Func<IMessage, bool, ISession, bool> ConfigAckBehaviour;

        /// <summary>
        /// Use this delegate to subscribe to the internal log of the queue handler.
        /// </summary>
        public Action<string, LogLevel> ConfigInternalLogBehaviour;
    }
}