using Cysharp.Threading.Tasks;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class InformPopUp : PopUp
    {
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private DefaultReactiveButton _closeBtn;

        public Subject<Unit> Closed { get; } = new Subject<Unit>();
        
        private void Awake()
        {
            _closeBtn.Clicked.Subscribe(unit => Closed.OnNext(Unit.Default)).AddTo(destroyCancellationToken);
        }

        public void SetDescription(string title, string description)
        {
            _titleText.text = title;
            _descriptionText.text = description;
        }
    }
}