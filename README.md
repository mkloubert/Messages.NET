# Messages.NET

A C# library for sending data via contracts between systems.

## Branches

| Name  | Targets on  |
| ----- | ----------- |
| master (current)  | C# 4.0, .NET 4.0, Mono  |
| [CSharp6](https://github.com/mkloubert/Messages.NET/tree/CSharp6)  | C# 6.0, .NET 4.6  |
| [NetCore5](https://github.com/mkloubert/Messages.NET/tree/NetCore5)  | C# 6.0, .NET Core 5  |

## Examples

### Address book

The example shows how you can synchronize contact data between two address books, like Outlook and Thunderbird.

First of all define a contract that stores data of a new contact entry.

```csharp
public interface INewContact {
    string Email { get; set; }
    
    string Firstname { get; set; }

    string Lastname { get; set; }
}
```

Now we should start with the implementation of the address books:

At the beginning we define a base class:

```csharp
abstract class AddressBook extends MessageHandlerBase {
    // create a new contact in that address book
    // and send it to the other address books
    public abstract void CreateContact(string firstName, string lastName,
                                       string email);
                          
    // receive a new contact from another address book             
    [ReceiveMessage]
    protected abstract void ReceiveNewContact(INewContact contact);
}
```

The next steps are to create implementations for `Outlook`

```csharp
class OutlookAddressBook : AddressBook {
    public override void CreateContact(string firstName, string lastName,
                                       string email) {
                                       
        // 1. create entry in Outlook
        // ...
        
        // 2. send new contact to the other address books (Thunderbird in that case)
        {
            var newMsg = Context.CreateNewMessage<INewContact>();
        
            // the "INewContact" object is stored
            // in "Message" property of "newMsg" object
            var newContact = newMsg.Message;
            newContact.Firstname = firstName;
            newContact.Lastname = lastName;
            newContact.Email = email;
            
            // send message to other
            // address books
            newMsg.Send();
        }
    }           
    
    protected override void ReceiveNewContact(INewContact contact) {
        // 1. create new entry in Outlook
    }
}
```

and `Thunderbird`:

```csharp
class ThunderbirdAddressBook : AddressBook {
    public override void CreateContact(string firstName, string lastName,
                                       string email) {
        // do the same thing as in "OutlookAddressBook"
        // for Thunderbird
    }
    
    protected override void ReceiveNewContact(INewContact contact) {
        // do the same thing as in "OutlookAddressBook"
        // for Thunderbird
    }
}
```

Create and set up an instance of `MessageDistributor` class for sharing new contact data between Outlook and Thunderbird:

```csharp
var outlook = new OutlookAddressBook();
var thunderbird = new ThunderbirdAddressBook();

var distributor = new MessageDistributor();

// register and set up Outlook
distributor.RegisterHandler(outlook)
           .RegisterForSend<INewContact>()
           .RegisterForReceive<INewContact>();

// register and set up Thunderbird           
distributor.RegisterHandler(thunderbird)
           .RegisterForSend<INewContact>()
           .RegisterForReceive<INewContact>();
```

Now lets share new contacts (from Outlook to Thunderbird):

```csharp
// create a new contact in Outlook
// and send it to Thunderbird
outlook.CreateContact("Marcel", "Kloubert",
                      "marcel.kloubert@gmx.net");
```

Or use the other direction (from Thunderbird to Outlook):

```csharp
thunderbird.CreateContact("Marcel", "Kloubert",
                          "marcel.kloubert@gmx.net");
```

And that's all you need to do to share data!

If you need an additional address book system, simply create an object based on `AddressBook` class and register it to the `MessageDistributor` object the same way as in that example.
