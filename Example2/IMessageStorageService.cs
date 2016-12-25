using Apache.NMS;

namespace Example2
{
    public interface IMessageStorageService
    {
        MessageHistoryItem GetMessageHistoryItem(string messageId);
        MessageHistoryItem AddOrUpdateMessageHistoryItem(IMessage message, bool isProcessed);
    }
}