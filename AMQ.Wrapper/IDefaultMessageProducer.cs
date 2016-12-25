using Apache.NMS;

namespace AMQ.Wrapper
{
    public interface IDefaultMessageProducer : IMessageProducer
    {
        string QueueUri { get; }

        string QueuePath { get; }

        QueueSettings QueueSettings { get; }

        ISession Session { get; }

        IConnection Connection { get; }

        IDestination Destination { get; }

        IConnectionFactory ConnectionFactory { get; }

        event ExceptionListener ConnectionExceptionListener;

        void StartProducer();

    }
}
