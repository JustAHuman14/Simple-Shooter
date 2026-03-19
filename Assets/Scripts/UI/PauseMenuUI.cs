using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameInput _gameInput;
        [SerializeField] private Button _mainMenuBtn;
        [SerializeField] private Button _resumeBtn;

        private void Start()
        {
            gameObject.SetActive(false);
            _gameInput.OnExit += ctx =>
            {
                gameObject.SetActive(true);
                Time.timeScale = 0;
            };

            _mainMenuBtn.onClick.AddListener(() => SceneManager.LoadScene(0));
            _resumeBtn.onClick.AddListener(() => GameResume());
        }

        private void GameResume()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
