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

Now you should define a message handler that sends the message:

```csharp
public class NewContactSender : MessageHandlerBase
{
    public void CreateContact(string firstName, string lastName)
    {
        // create a new instance of "INewContact"
        // that is wrapped into a context object
        //
        // the new "INewContact" instance is stored
        // in "Message" property
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
        // the "INewContact" is wrapped and stored
        // in "Message" property
    
        Console.WriteLine("Lastname: {0}, Firstname: {1}",
                          msg.Message.Lastname, msg.Message.Firstname);
    }
    
    protected override void OnContextUpdated(IMessageHandlerContext oldCtx, IMessageHandlerContext newCtx)
    {
        base.OnContextUpdated(oldCtx, newCtx);

        // subscribe "HandleNewContact" for receiving
        // "INewContact" objects
        newCtx.Subscribe<INewContact>(HandleNewContact);
    }
}
```

The class `MessageDistributor` helps you to send messages between handlers:

```csharp
var sender = new NewContactSender();
var receiver = new NewContactReceiver();

var distributor = new MessageDistributor();

// register the "sender" in "distributor"
// and configure it for sending
var senderCfg = distributor.RegisterHandler(sender);
senderCfg.RegisterForSend<INewContact>();

// register the "receiver" in "distributor"
// and configure it for receiving
var receiverCfg = distributor.RegisterHandler(receiver);
receiverCfg.RegisterForReceive<INewContact>();

// now create a new "INewContact" instance
// and send it to "receiver"
sender.CreateContact("Marcel", "Kloubert");
```

Keep in mind, that it does not make sense to define other things as proprties in interface based contracts, because the `MessageDistributor` will create dynamic proxy classes by default.

If you want to define other things like methods, e.g., you have to define a non-abstract/-static class as contract or define an own proxy class. 

#### The attribute way

Instead of calling `Subscribe` method in an `MessageHandlerBase` object, you can use the `ReceiveMessageAttribute` to do this.

This makes your receiver class much more compact:

```csharp
public class NewContactReceiver : MessageHandlerBase
{
    [ReceiveMessage(typeof(INewContact))]
    protected void HandleNewContact(IMessageContext<INewContact> msg)
    {
        // ...
    }
}
```

Or shorter (without type argument):

```csharp
public class NewContactReceiver : MessageHandlerBase
{
    [ReceiveMessage]
    protected void HandleNewContact(IMessageContext<INewContact> msg)
    {
         // ...
    }
}
```

#### Own proxy class

If you want to define an own proxy class for a contract, you can use `MessageInstanceAttribute` for that.

First define the proxy class:

```csharp
public class NewContact : INewContact
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }
}
```

Then mark the contract with the attribute:

```csharp
[MessageInstance(typeof(NewContact))]
public interface INewContact
{
    string Firstname { get; set; }

    string Lastname { get; set; }
}
```
