using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameInput _gameInput;
        [SerializeField] private Button _yesBtn;
        [SerializeField] private Button _noBtn;

        private void Start()
        {
            gameObject.SetActive(false);
            _gameInput.OnExit += ctx =>
            {
                gameObject.SetActive(true);
                Time.timeScale = 0;
            };

            _yesBtn.onClick.AddListener(() => Application.Quit());
            _noBtn.onClick.AddListener(() => GameResume());
        }

        private void GameResume()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
