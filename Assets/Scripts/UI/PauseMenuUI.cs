using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        private GameInput _gameInput;
        [SerializeField] private Button _mainMenuBtn;
        [SerializeField] private Button _resumeBtn;
        [SerializeField] private Slider _mouseSensitivitySlider;

        private void Start()
        {
            _gameInput = GlobalReferences.Instance.gameInput;
            gameObject.SetActive(false);
            _gameInput.OnExit += ctx =>
            {
                gameObject.SetActive(true);
                Time.timeScale = 0;
            };

            _mainMenuBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(0);
                Time.timeScale = 1;
            });

            _resumeBtn.onClick.AddListener(() => GameResume());
            _mouseSensitivitySlider.onValueChanged.AddListener(ctx => GameManager.mouseSensitivity = ctx);
        }

        private void GameResume()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
