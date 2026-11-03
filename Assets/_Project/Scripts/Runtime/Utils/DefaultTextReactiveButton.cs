using TMPro;
using UnityEngine;

namespace Scripts.Runtime.Utils
{
    public class DefaultTextReactiveButton : DefaultReactiveButton
    {
        [SerializeField] protected TMP_Text _text;

        public void SetText(string toString)
        {
            _text.text = toString;
        }
    }
}