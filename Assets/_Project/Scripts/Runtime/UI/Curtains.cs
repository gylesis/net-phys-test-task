using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class Curtains : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            HideCurtains();
        }

        public void ShowCurtains()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
        }

        public void HideCurtains()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}