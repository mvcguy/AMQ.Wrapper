# AMQ.Wrapper
This wrapper is build to simplify the process of creating a queue consumer or a producer. Sample projects have been added to demonstrate this. The wrapper is flexible enough to adapt to your personal business needs. I hope it will be benenficial and useful for your projects. Contact me at @ me.shahidali@hotmail.com for any queries. Thanks enjoy!
# Consumer Simple Usage
```csharp
var queueHandler = new DefaultQueueHandler("activemq:tcp://localhost:61616", "amq.test1",
                new List<IMessageHandler>() { new SampleMessageHandler() });

queueHandler.StartSession();
```
# Consumer Advance Usage
```csharp
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
```
# Message Producer Sample
```csharp
IDefaultMessageProducer producer = new DefaultMessageProducer(uri, path);
producer.DeliveryMode = MsgDeliveryMode.Persistent;
producer.StartProducer();

// Send a message
var message = producer.Session.CreateTextMessage(DateTime.Now.ToString("T") + ": Hello World!");
message.NMSCorrelationID = Guid.NewGuid().ToString();
message.Properties["NMSXGroupID"] = "Group-" + Guid.NewGuid();
message.Properties["myHeader"] = "Header-" + DateTime.Now.ToString("T");
producer.Send(message);
```
# ### [Nuget] (https://www.nuget.org/packages/AMQ.Wrapper/ "Nuget") ###

PM> Install-Package AMQ.Wrapper
