using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class SessionPlayerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nicknameText;
        [SerializeField] private TMP_Text _pingText;

        [SerializeField] private DefaultReactiveButton _kickBtn;

        public Subject<Unit> KickBtnClicked => _kickBtn.Clicked;
            
        public ulong ClientId { get; set; }

        public void SetKickBtnEnabled(bool enabled)
        {
            _kickBtn.gameObject.SetActive(enabled);
        }
        
        public void SetNickname(string nickname)
        {
            _nicknameText.text = nickname;
        }

        public void SetPing(string ping)
        {
            _pingText.text = ping;
        }
            
    }
}