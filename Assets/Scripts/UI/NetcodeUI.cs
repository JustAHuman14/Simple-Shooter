using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Assets.Scripts.UI
{
    public class NetcodeUI : MonoBehaviour
    {
        [SerializeField] private Button _startHostBtn;
        [SerializeField] private Button _startClientBtn;

        private void Start()
        {
            _startHostBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost(); 
            });

            _startClientBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            });
        }
    }
}
