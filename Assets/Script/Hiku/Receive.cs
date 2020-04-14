using System;
using System.Runtime.CompilerServices;

namespace Hiku
{
    /// <summary>
    /// Method that is listening a data provider.
    /// </summary>
    public class Receive : UnityEngine.Scripting.PreserveAttribute
    {
        /// <summary>
        /// Specifies the order in which data will be received when the component is initialized.
        /// Depends on the order receiving methods are declared.
        /// </summary>
        public int Order { get; private set; }

        public Receive([CallerLineNumber] int order = 0)
            => Order = order;
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