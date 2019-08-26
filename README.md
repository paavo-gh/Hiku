# Hiku

Receiver components inherit from ReceiverComponent and receive data through methods 
tagged with [Receive] -attribute.

Here is a simple component that expects a string and assigns it 
into a text field.

```c#
public class TextDisplay : ReceiverComponent
{
    [SerializeField] TMP_Text textField = null;

    [Receive] void SetText(string text)
    {
        textField.text = text;
    }
}
```
![TextDisplay](Docs/TextDisplay.png)

The component above will, by default, receive its data from the closest parent game object  
that provides the data of type string. Provider components inherit from ProviderComponent 
and provide data through DataFields. Here is an example:

```c#
public class CharacterDataProvider : ProviderComponent
{
    DataField<CharacterData> characterData;

    protected override void Create()
    {
        characterData.Set(new CharacterData());
    }
}
```
![CharacterDataProvider](Docs/CharacterDataProvider.png)

In this case, however, the component is not providing data of type string, 
but CharacterData instead. Here is the class:

```c#
[Receivable]
public class CharacterData
{
    public string Name { get; private set; }
}
```

Since CharacterData has a field of type string, it means we can hook 
it up with the TextField-component in the editor.

![TextDisplay](Docs/TextDisplaySet.png)

If we wanted to have the TextField also change its data every time a character's 
name changes, we could do so by defining the Name-field as DataField in CharacterData:

```c#
[Receivable]
public class CharacterData
{
    public DataField<string> Name { get; private set; }
}
```

Receivable-attribute is there just to tell the editor scripts to list all the fields 
of the class as receivable data.

There is also ProviderReceiverComponent when you need a component that can do both.

## Unity Events

Receiver and provider components inherit from MonoBehaviour and introduce some methods to 
hook for common Unity events: Create, Enable, Disable. Use these methods 
instead of Unity's OnEnable, OnDisable or Awake, as these are already implemented 
in the base class.

```c#
protected override void Create()
{
    // Called once when the component has been created.
    // Receivers: Guarantees that all available data has been received.
    // Providers: Called before providing any data to the receivers.
}

protected override void Enable()
{
    // Executes once after Create and every time the component is enabled.
}

protected override void Disable()
{
    // Executes every time the component is disabled.

    // Component will not receive any data updates while it is disabled, 
    // Any data that changes while the component is disabled, 
    // will be received the moment the component is enabled again.
}
```
