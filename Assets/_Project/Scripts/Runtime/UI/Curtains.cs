using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class Curtains : MonoBehaviour
    {
        public static Curtains Instance { get; private set; }
        
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            Instance = this;
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