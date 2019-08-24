using TMPro;
using UnityEngine;

namespace Hiku.Examples.Simple
{
    public class TextDisplay : ReceiverComponent
    {
        [SerializeField] TMP_Text textField = null;

        // Receives value from a parent component
        [Receive] void SetText(string text)
        {
            textField.text = text;
        }
    }
}