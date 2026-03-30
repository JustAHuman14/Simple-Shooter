using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

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
            _mouseSensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity");
            _gameInput = GlobalReferences.Instance.gameInput;
            gameObject.SetActive(false);

            _gameInput.OnExit += ToggleMenu;

            _mainMenuBtn.onClick.AddListener(() =>
            {
                PlayerPrefs.SetFloat("mouseSensitivity", GameManager.mouseSensitivity);
                SceneChanger.Instance.LoadScene(0);
                Time.timeScale = 1;
            });

            _resumeBtn.onClick.AddListener(() => GameResume());
            _mouseSensitivitySlider.onValueChanged.AddListener(ctx => GameManager.mouseSensitivity = ctx);
        }

        private void ToggleMenu(InputAction.CallbackContext context)
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);
            Time.timeScale = gameObject.activeInHierarchy ? 0 : 1;
            Cursor.lockState = gameObject.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void GameResume()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            if (_gameInput != null)
            {
                _gameInput.OnExit -= ToggleMenu;
            }
        }
    }
}
