using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Apache.NMS;

namespace AMQ.Wrapper.Receiver
{
    /// <summary>
    /// A wrapper class around AMQ API to simplify the whole process.
    /// <para>Support session reset policy which is important for long running processes to start consuming
    /// messages that were previously un-acknowledged due to errors.</para>
    /// <para>Here you can specify several handlers which will be called when-ever a message is arrived</para>
    /// <para>All the functionality is extensible, meaning the child classes can override the default behaviour according to business requirements</para>
    /// </summary>
    public class DefaultQueueHandler : IQueueHandler
    {
        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        public event ExceptionListener ConnectionExceptionListener;

        protected SessionResetPolicy ResetPolicy;

        protected IList<IMessageHandler> MessageHandlers;

        protected IDefaultMessageConsumer MessageConsumer;

        public DateTime SessionStartedOn { get; protected set; }

        protected static readonly object Sync = new object();

        protected bool IsSessionStarted;

        protected Thread SessionResetPolicyThread;

        /// <summary>
        /// Use this to modify the default configuration of queue session, connection, connection factory, destination and consumer
        /// </summary>
        public QueueSettings AdvancedSettings { get; private set; }

        public DefaultQueueHandler(
            string queueUri,
            string queuePath,
            IList<IMessageHandler> messageHandlers,
            SessionResetPolicy resetPolicy = null) :
            this(new DefaultMessageConsumer(queueUri, queuePath), messageHandlers, resetPolicy)
        { }

        public DefaultQueueHandler(
            IDefaultMessageConsumer messageConsumer,
            IList<IMessageHandler> messageHandlers,
            SessionResetPolicy resetPolicy = null)
        {
            MessageConsumer = messageConsumer;

            MessageHandlers = messageHandlers;

            ResetPolicy = resetPolicy ?? new SessionResetPolicy();

            SessionResetPolicyThread = new Thread(ApplySessionResetPolicy);

            AdvancedSettings = messageConsumer.QueueSettings;
        }

        /// <summary>
        /// This will start a new queue session and it will trigger the Handle method of all the registered handlers.
        /// Once a session is started, repeated calls to this method will be ignored
        /// This method also start the SessionResetPolicyThread
        /// </summary>
        public virtual void StartSession()
        {
            SessionStarting();

            SessionStarted();

            StartSessionResetPolicyService();

        }

        /// <summary>
        /// This method setup the listener and start the consumer to listen to incoming messages
        /// </summary>
        protected virtual void SessionStarting()
        {
            if (IsSessionStarted)
            {
                LogEntry("INFO | Session is already started!");
                return;
            }

            MessageConsumer.Listener += MessageConsumer_Listener;

            if (ConnectionExceptionListener != null)
            {
                MessageConsumer.ConnectionExceptionListener += ConnectionExceptionListener;
            }

            lock (Sync)
            {
                MessageConsumer.StartConsumer();
            }

            SessionStartedOn = DateTime.Now;

            IsSessionStarted = true;
        }

        /// <summary>
        /// this method is called after the session is started successfully
        /// </summary>
        protected virtual void SessionStarted()
        {
            LogEntry("INFO | Session is started successfully.");

            lock (Sync)
            {
                LogEntry(string.Format("INFO | QueuePath: '{0}', QueueUri: '{1}'.", MessageConsumer.QueuePath, MessageConsumer.QueueUri));
            }

            LogEntry("INFO | Listening to any new incoming messages...\n");
        }

        /// <summary>
        /// the session reset policy service will only be started if <see cref="SessionResetPolicy.ShouldResetSession"/>
        /// is set to true and <see cref="SessionResetPolicy.ResetFrequencyHours"/> has a value greater than zero
        /// </summary>
        protected virtual void StartSessionResetPolicyService()
        {
            if (ResetPolicy.ShouldResetSession && ResetPolicy.ResetFrequencyHours > 0)
            {
                SessionResetPolicyThread.Start();
            }
        }

        /// <summary>
        /// runs after one minutes to enforce session reset policy
        /// session reset is important to consume messages that were previously un-handled/un-acknowledged
        /// </summary>
        protected virtual void ApplySessionResetPolicy()
        {
            try
            {
                LogEntry("INFO | Session reset policy is running...");

                while (true)
                {
                    lock (Sync)
                    {
                        var resetSession = DateTime.Now >= SessionStartedOn.AddHours(ResetPolicy.ResetFrequencyHours);

                        if (resetSession)
                        {
                            ResetSession();
                        }
                    }
                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException)
            {
                LogEntry("INFO | Session reset policy service/thread is being stopped...");
            }
        }

        /// <summary>
        /// reset the consumer and start over the session again
        /// this method is called when its time to reset the session according to the reset policy
        /// the timing is monitored by ApplySessionResetPolicy method, which is running inside the thread SessionResetPolicyThread
        /// </summary>
        protected virtual void ResetSession()
        {
            LogEntry("INFO | Session is being reset...");
            try
            {
                MessageConsumer.CloseConnection();
                MessageConsumer.StartConsumer();
                SessionStartedOn = DateTime.Now;
                LogEntry("INFO | Session is resetted successfully...");
            }
            catch (Exception e)
            {
                LogEntry(string.Format("ERROR | Error resetting session. Error: '{0}'", e.Message), LogLevel.Error);
            }
        }

        /// <summary>
        /// this method is called everytime a new message is received
        /// </summary>
        /// <param name="message"></param>
        protected virtual void MessageConsumer_Listener(IMessage message)
        {
            //lock is necessary to avoid conflict between ApplySessionResetPolicy and this method
            lock (Sync)
            {
                var handled = false;
                var acked = false;
                try
                {
                    //do some pre-processing on the message
                    MessageReceived(message);

                    handled = HandleMessage(message);

                    //once Acknowledged the message is removed from the queue
                    acked = AcknowledgeMessage(message, handled, MessageConsumer.Session);

                    //do post processing
                    MessageProcessed(message, handled, acked);

                }
                catch (Exception ex)
                {
                    ErrorInConsumingMessage(message, ex, handled, acked);
                }
            }
        }

        /// <summary>
        /// the method is called when there is exception during the processing of the message.
        /// If the error occurs after the message was acknowledged, then the message is already removed from the queue,
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="handled"></param>
        /// <param name="acknowledged"></param>
        protected virtual void ErrorInConsumingMessage(IMessage message, Exception exception, bool handled, bool acknowledged)
        {
            LogEntry(string.Format("ERROR | error occurred while processing message '{0}' Error: {1}", message.NMSCorrelationID, exception.Message), LogLevel.Error);
        }

        /// <summary>
        /// called after all the processing is done to the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="handled"></param>
        /// <param name="acknowledged"></param>
        protected virtual void MessageProcessed(IMessage message, bool handled, bool acknowledged)
        {
            if (handled && acknowledged)
            {
                LogEntry(string.Format("INFO | Message '{0}' is handled and acknowledged.", message.NMSCorrelationID));
            }

            if (handled && !acknowledged)
            {
                LogEntry(string.Format("INFO | Message '{0}' is handled but not acknowledged.", message.NMSCorrelationID));
            }

            if (!handled && acknowledged)
            {
                LogEntry(string.Format("INFO | Message '{0}' is acknowledged but not handled.", message.NMSCorrelationID));
            }

            if (!(handled || acknowledged))
            {
                LogEntry(string.Format("INFO | Message '{0}' is neither acknowledged nor handled.", message.NMSCorrelationID));
            }

        }

        /// <summary>
        /// called after the message is processed by the handlers
        /// Here you have the choice to ack the message or ignore it. once ack'ed the message will be removed from queue
        /// Return true if you have choosen to ack the message, otherwise return false.
        /// <para>
        /// Alternatively call session.commit to commit this message and all the previous message received during the session. 
        /// In the later case, you might need to initialize the session with AcknowledgementMode.Transactional
        /// <para>It is recommended to use <see cref="AcknowledgementMode.IndividualAcknowledge"/> to have fine grain control over each message</para>
        /// </para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="handled"></param>
        /// <param name="session"></param>
        protected virtual bool AcknowledgeMessage(IMessage message, bool handled, ISession session)
        {
            if (AdvancedSettings.ConfigAckBehaviour != null)
            {
                return AdvancedSettings.ConfigAckBehaviour.Invoke(message, handled, session);
            }

            if (!handled) return false;

            switch (session.AcknowledgementMode)
            {
                case AcknowledgementMode.AutoAcknowledge:
                case AcknowledgementMode.DupsOkAcknowledge:
                    //messages are automatically acknowledged
                    break;

                case AcknowledgementMode.ClientAcknowledge:
                case AcknowledgementMode.IndividualAcknowledge:
                    message.Acknowledge();
                    break;
                case AcknowledgementMode.Transactional:
                    session.Commit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        /// <summary>
        /// this method is called as soon as the message is arrived. 
        /// so if you need to perform some pre-processing on the message, this is where it should go
        /// </summary>
        /// <param name="message"></param>
        protected virtual void MessageReceived(IMessage message)
        {
            LogEntry(string.Format("INFO | Message '{0}' arrived", message.NMSCorrelationID));
        }

        /// <summary>
        /// returning true from this method will casue the message to be acknowledged and removed from queue
        /// </summary>
        /// <param name="message"></param>
        /// <param name="historyItem"></param>
        /// <returns></returns>
        protected virtual bool HandleMessage(IMessage message)
        {
            var processStatus = new Dictionary<string, bool>();

            foreach (var messageHandler in MessageHandlers)
            {
                //TODO: consider async task for each handler
                var processed = messageHandler.HandleMessage(message);
                processStatus.Add(messageHandler.HandlerName, processed.GetValueOrDefault(false));
            }

            //message should be handled by at least one handler to acknowledge
            var handled = processStatus.Values.Any(x => x == true);

            return handled;
        }

        protected virtual void LogEntry(string message, LogLevel level = LogLevel.Info)
        {
            if (AdvancedSettings.ConfigInternalLogBehaviour != null)
            {
                AdvancedSettings.ConfigInternalLogBehaviour.Invoke(message, level);
            }
            else
            {
                switch (level)
                {
                    case LogLevel.Error:
                        Debug.WriteLine(message, "Error");
                        break;
                    case LogLevel.Info:
                        Debug.WriteLine(message, "Info");
                        break;
                    case LogLevel.Warning:
                        Debug.WriteLine(message, "Warning");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
        }

        /// <summary>
        /// this will also abort the SessionResetPolicyThread
        /// If overriden, make sure SessionResetPolicyThread is aborted here, or somewhere before disposing
        /// </summary>
        public virtual void Dispose()
        {
            if (SessionResetPolicyThread != null)
            {
                try
                {
                    SessionResetPolicyThread.Abort();
                }
                catch (Exception ex)
                {
                    LogEntry(ex.Message);
                }
            }
            if (MessageConsumer != null)
            {
                MessageConsumer.Dispose();
            }

        }
    }
}