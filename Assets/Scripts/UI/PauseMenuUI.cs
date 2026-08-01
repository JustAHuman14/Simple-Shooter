using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.UI
{
    // ReSharper disable once InconsistentNaming
    public class PauseMenuUI : MonoBehaviour
    {
        private GameInput _gameInput;
#pragma warning disable CS0649
        [SerializeField] private Button _mainMenuBtn;
        [SerializeField] private Button _resumeBtn;
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private Toggle _postProcessingToggle;
        [SerializeField] private UniversalAdditionalCameraData _playerCameraData;
#pragma warning restore CS0649

        [UsedImplicitly]
        private void Start()
        {
            _mouseSensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity");
            _gameInput = GlobalReferences.Instance.GameInput;
            gameObject.SetActive(false);

            _gameInput.OnExit += ToggleMenu;

            _mainMenuBtn.onClick.AddListener(() =>
            {
                PlayerPrefs.SetFloat("mouseSensitivity", GameManager.MouseSensitivity);
                SceneChanger.Instance.LoadScene(0);
                Time.timeScale = 1;
            });

            _resumeBtn.onClick.AddListener(GameResume);
            _mouseSensitivitySlider.onValueChanged.AddListener(ctx => GameManager.MouseSensitivity = ctx);
        }

        [UsedImplicitly]
        private void Update() => _playerCameraData.renderPostProcessing = _postProcessingToggle.isOn;

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

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (_gameInput != null)
            {
                _gameInput.OnExit -= ToggleMenu;
            }
        }
    }
}
