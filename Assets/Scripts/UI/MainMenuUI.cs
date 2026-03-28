using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Assets.Scripts.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _loading;

        private void Awake()
        {
            _startButton.onClick.AddListener(() =>
            {
                _loading.SetActive(true);
                SceneManager.LoadScene(1);
                NetworkManager.Singleton.StartHost();
//                NetworkManager.Singleton.StartClient();
            });

            _quitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}
