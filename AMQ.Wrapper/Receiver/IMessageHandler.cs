using Apache.NMS;

namespace AMQ.Wrapper.Receiver
{
    public interface IMessageHandler
    {
        /// <summary>
        /// This method will be called whenever a new message is arrived
        /// It is important to return an appropriate value from this method.
        /// <para>To ingore a message return a null response</para>
        /// <para>To acknowledge a message, return a true value, this will cause the message to be removed from the queue</para>
        /// <para>If the message cannot be handled at the moment, return a false value. This will prevent the message from
        /// being acknowledged, and in this way, it will remain in the queue</para>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool? HandleMessage(IMessage message);

        string HandlerName { get; }
    }
}