using System;

namespace Hiku
{
    /// <summary>
    /// Method that is listening a data provider.
    /// </summary>
    public class Receive : Attribute
    {
    }

    /// <summary>
    /// Data objects with [Receivable]-attribute can have their getter methods 
    /// selectable as a receivable type in the editor. This is just to limit 
    /// the available options.
    /// </summary>
    public class Receivable : Attribute
    {
    }
}