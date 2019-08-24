using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hiku.Examples.Simple
{
    public class ChangeNameInput : ReceiverComponent
    {
        // Receives value from a parent object
        IPlayerService playerService { get; [Receive] set; }

        [SerializeField] TMP_InputField inputField = null;

        void Start()
        {
            inputField.onSubmit.AddListener(playerService.ChangeName);
        }
    }
}