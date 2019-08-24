using System;

namespace Hiku
{
    /// <summary>
    /// Method that is listening a data provider.
    /// </summary>
    public class Receive : UnityEngine.Scripting.PreserveAttribute
    {
        /// <summary>
        /// Specifies the order in which data should be 
        /// received when the component is initialized.
        /// Higher will receive later. Defaults to zero.
        /// </summary>
        public int Order { get; set; }
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