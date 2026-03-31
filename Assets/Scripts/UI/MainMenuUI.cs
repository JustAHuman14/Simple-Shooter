using UnityEngine.UI;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            _startButton.onClick.AddListener(() =>
            {
                SceneChanger.Instance.LoadScene(1);
            });

            _quitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}
