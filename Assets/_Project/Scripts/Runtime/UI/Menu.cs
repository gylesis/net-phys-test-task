using UnityEngine;

namespace Scripts.Runtime.UI
{
    public abstract class Menu : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;

        public bool IsVisible { get; private set; }

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Awake() { }

        protected virtual void Start() { }

        public virtual void Show()
        {
            EnableSmooth();
        }

        public virtual void Hide()
        {
            DisableSmooth();
        }

        protected void EnableCanvasGroup()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        protected void DisableCanvasGroup()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        protected void EnableSmooth()
        {
            EnableCanvasGroup();
            SetCanvasAlpha(1);
        }

        protected void DisableSmooth()
        {
            DisableCanvasGroup();
            SetCanvasAlpha(0);
        }

        protected void SetDisableState(bool isDisable)
        {
            SetCanvasAlpha(isDisable ? 0 : 1);
            _canvasGroup.interactable = !isDisable;
            _canvasGroup.blocksRaycasts = !isDisable;
        }

        protected void SetCanvasAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
            IsVisible = alpha > 0;
        }
    }
}