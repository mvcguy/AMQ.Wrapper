using System;
using Apache.NMS;

namespace AMQ.Wrapper
{
    /// <summary>
    /// a wrapper class around <see cref="IMessageConsumer"/> to simplify the process
    /// of creating a message consumer.
    /// </summary>
    public class DefaultMessageConsumer : IDefaultMessageConsumer
    {
        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public event MessageListener Listener;

        public string QueueUri { get; private set; }

        public string QueuePath { get; private set; }

        protected IMessageConsumer MessageConsumer;

        public IDestination Destination { get; protected set; }

        public ISession Session { get; protected set; }

        public IConnection Connection { get; protected set; }

        public IConnectionFactory ConnectionFactory { get; protected set; }

        public QueueSettings QueueSettings { get; private set; }

        public event ExceptionListener ConnectionExceptionListener;

        public DefaultMessageConsumer(string queueUri, string queuePath, QueueSettings settings = null)
        {
            QueueSettings = settings ?? new QueueSettings();
            QueueUri = queueUri;
            QueuePath = queuePath;
        }

        /// <summary>
        /// Use to method to start listening to the queue. This method initializes queue session, connection, connection factory, consumer and destination 
        /// </summary>
        public virtual void StartConsumer()
        {
            InitConnectionFactory();
            InitConnection();
            InitSession();
            InitQueue();
            InitConsumer();

            if (Listener != null)
            {
                MessageConsumer.Listener += Listener;
            }
            if (ConsumerTransformer != null)
            {
                MessageConsumer.ConsumerTransformer += ConsumerTransformer;
            }

        }

        protected virtual void InitConsumer()
        {
            MessageConsumer = QueueSettings.ConfigMessageConsumer != null
                ? QueueSettings.ConfigMessageConsumer.Invoke(Session, Destination)
                : Session.CreateConsumer(Destination);
        }

        protected virtual void InitQueue()
        {
            Destination = QueueSettings.ConfigDestinationFunc != null
                ? QueueSettings.ConfigDestinationFunc.Invoke(QueuePath, Session)
                : Session.GetQueue(QueuePath);
        }

        protected virtual void InitSession()
        {
            Session = QueueSettings.ConfigSessionFunc != null
                ? QueueSettings.ConfigSessionFunc.Invoke(Connection)
                : Connection.CreateSession(AcknowledgementMode.IndividualAcknowledge);
        }

        /// <summary>
        /// this method will initialize and start connection for the queue. It also registers <see cref="ConnectionExceptionListener"/>.
        /// </summary>
        protected virtual void InitConnection()
        {
            Connection = QueueSettings.ConfigConnectionFunc != null
                ? QueueSettings.ConfigConnectionFunc.Invoke(ConnectionFactory)
                : ConnectionFactory.CreateConnection();

            if (ConnectionExceptionListener != null)
            {
                Connection.ExceptionListener += ConnectionExceptionListener;
            }

            Connection.Start();
        }

        protected virtual void InitConnectionFactory()
        {
            ConnectionFactory = QueueSettings.ConfigFactoryFunc != null
                ? QueueSettings.ConfigFactoryFunc.Invoke(QueueUri)
                : new NMSConnectionFactory(QueueUri);
        }

        public virtual void Dispose()
        {
            if (MessageConsumer != null)
            {
                MessageConsumer.Dispose();
            }

            if (Session != null)
            {
                Session.Dispose();
            }

            if (Connection != null)
            {
                Connection.Dispose();
            }

            if (Destination != null)
            {
                Destination.Dispose();
            }
        }

        public virtual IMessage Receive()
        {
            return MessageConsumer.Receive();
        }

        public virtual IMessage Receive(TimeSpan timeout)
        {
            return MessageConsumer.Receive(timeout);
        }

        public virtual IMessage ReceiveNoWait()
        {
            return MessageConsumer.ReceiveNoWait();
        }

        public virtual void Close()
        {
            MessageConsumer.Close();
        }

        public virtual void ResetSession()
        {
            Session.Recover();
        }

        public virtual void CloseConnection()
        {
            Connection.Close();
        }

    }
}