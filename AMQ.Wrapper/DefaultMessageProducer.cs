using System;
using Apache.NMS;

namespace AMQ.Wrapper
{
    public class DefaultMessageProducer : DefaultMessageConsumer, IDefaultMessageProducer
    {
        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        public MsgDeliveryMode DeliveryMode { get; set; }

        public TimeSpan TimeToLive { get; set; }

        public TimeSpan RequestTimeout { get; set; }

        public MsgPriority Priority { get; set; }

        public bool DisableMessageID { get; set; }

        public bool DisableMessageTimestamp { get; set; }

        protected IMessageProducer MessageProducer;

        public DefaultMessageProducer(string queueUri, string queuePath, QueueSettings queueSettings = null) : base(queueUri, queuePath, queueSettings)
        {
            
        }

        public virtual void Send(IMessage message)
        {
            MessageProducer.Send(message);
        }

        public virtual void Send(IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority, TimeSpan timeToLive)
        {
            MessageProducer.Send(message, deliveryMode, priority, timeToLive);
        }

        public virtual void Send(IDestination destination, IMessage message)
        {
            MessageProducer.Send(destination, message);
        }

        public virtual void Send(IDestination destination, IMessage message, MsgDeliveryMode deliveryMode, MsgPriority priority,
            TimeSpan timeToLive)
        {
            MessageProducer.Send(destination, message, deliveryMode, priority, timeToLive);
        }

        public virtual IMessage CreateMessage()
        {
            return MessageProducer.CreateMessage();
        }

        public virtual ITextMessage CreateTextMessage()
        {
            return MessageProducer.CreateTextMessage();
        }

        public virtual ITextMessage CreateTextMessage(string text)
        {
            return MessageProducer.CreateTextMessage(text);
        }

        public virtual IMapMessage CreateMapMessage()
        {
            return MessageProducer.CreateMapMessage();
        }

        public virtual IObjectMessage CreateObjectMessage(object body)
        {
            return MessageProducer.CreateObjectMessage(body);
        }

        public virtual IBytesMessage CreateBytesMessage()
        {
            return MessageProducer.CreateBytesMessage();
        }

        public virtual IBytesMessage CreateBytesMessage(byte[] body)
        {
            return MessageProducer.CreateBytesMessage(body);
        }

        public virtual IStreamMessage CreateStreamMessage()
        {
            return MessageProducer.CreateStreamMessage();
        }

        public override void Close()
        {
            MessageProducer.Close();
        }

        public override IMessage Receive()
        {
            throw new NotImplementedException("");
        }

        public override void StartConsumer()
        {
            throw new NotImplementedException();
        }

        protected override void InitConsumer()
        {
            throw new NotImplementedException();
        }

        public override IMessage ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public override void ResetSession()
        {
            throw new NotImplementedException();
        }

        public virtual void StartProducer()
        {
            InitConnectionFactory();
            InitConnection();
            InitSession();
            InitQueue();

            MessageProducer = QueueSettings.ConfigMessageProducer != null
                ? QueueSettings.ConfigMessageProducer.Invoke(Session, Destination)
                : Session.CreateProducer(Destination);
            
            MessageProducer.DeliveryMode = DeliveryMode;
            MessageProducer.TimeToLive = TimeToLive;
            MessageProducer.RequestTimeout = RequestTimeout;
            MessageProducer.Priority = Priority;
            MessageProducer.DisableMessageID = DisableMessageID;
            MessageProducer.DisableMessageTimestamp = DisableMessageTimestamp;
            MessageProducer.ProducerTransformer += ProducerTransformer;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (MessageProducer != null)
            {
                MessageProducer.Dispose();
            }
        }
    }
}