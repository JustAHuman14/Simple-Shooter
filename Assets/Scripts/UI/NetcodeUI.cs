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
            NetworkManager.Singleton.StartClient();
        }
    }
}
