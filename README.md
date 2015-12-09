# Messages.NET

A C# library for sending data via contracts between systems.

## Branches

| Name  | Targets on  |
| ----- | ----------- |
| master (current)  | C# 4.0, .NET 4.0, Mono  |

## Examples

### New contact

First of all, you have to define a message contract:

```csharp
public interface INewContact
{
    string Firstname { get; set; }

    string Lastname { get; set; }
}
```

Now you should define a message handler that sends the message. 

```csharp
public class NewContactSender : MessageHandlerBase
{
    public void CreateContact(string firstName, string lastName)
    {
        var newMsgCtx = Context.CreateMessage<INewContact>();
        newMsgCtx.Message.Firstname = firstname;
        newMsgCtx.Message.Lastname = lastName;
        
        newMsgCtx.Send();
    }
}
```

The next step is to define a message handler that receives a message:

```csharp
public class NewContactReceiver : MessageHandlerBase
{
    protected void HandleNewContact(IMessageContext<INewContact> msg)
    {
        Console.WriteLine("Lastname: {0}, Firstname: {1}",
                          msg.Lastname, msg.Firstname);
    }
    
    protected override void OnContextUpdated(IMessageHandlerContext oldCtx, IMessageHandlerContext newCtx)
    {
        base.OnContextUpdated(oldCtx, newCtx);

        newCtx.Subscribe<INewContact>(HandleNewContact);
    }
}
```

The class `MessageDistributor` helps you to send messages between handlers:

```csharp
var sender = new NewContactSender();
var receiver = new NewContactReceiver();

var distributor = new MessageDistributor();

// register the "sender" in distributor
// and configure it
var senderCfg = distributor.RegisterHandler(sender);
senderCfg.RegisterForSend<ITestMessage>();

// register the "receiver" in distributor
// and configure it
var receiverCfg = distributor.RegisterHandler(receiver);
receiverCfg.RegisterForReceive<ITestMessage>();

// now create a new INewContact instance
// and send it to receiver
sender.CreateContact("Marcel", "Kloubert");
```
