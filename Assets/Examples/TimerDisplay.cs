using System;
using TMPro;
using UnityEngine;

namespace Hiku.Examples
{
    public class TimerDisplay : ReceiverComponent
    {
        [SerializeField] TMP_Text textField = null;

        public DateTime EndsAt { get; [Receive] set; }
        ITimeComponent time { get; [Receive] set; }
        int totalSeconds = -1;

        void Update()
        {
            var timeLeft = EndsAt - time.CurrentTime;
            if (totalSeconds != (totalSeconds = (int) timeLeft.TotalSeconds))
                textField.text = GetTimeLeftText(timeLeft);
        }

        public static string GetTimeLeftText(TimeSpan timeLeft)
        {
            if (timeLeft.Days > 0)
                return string.Format("{0} Days", (int) timeLeft.Days);
            if (timeLeft.Hours > 0)
                return string.Format("{0}h{1:D2}m", (int) timeLeft.Hours, (int) timeLeft.Minutes);
            if (timeLeft.Minutes > 0)
                return string.Format("{0}:{1:D2}", (int) timeLeft.Minutes, (int) timeLeft.Seconds);
            if (timeLeft.Seconds > 0)
                return string.Format("{0}s", (int) timeLeft.Seconds);
            return "";
        }
    }
}