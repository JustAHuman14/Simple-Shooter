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
        [SerializeField] private Slider _mouseSensitivitySlider;

	private void Awake() => _mouseSensitivitySlider.value = GameManager.mouseSensitivity/100;
	
        private void Start()
        {
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
            _mouseSensitivitySlider.onValueChanged.AddListener(ctx => GameManager.mouseSensitivity = ctx / 1 * 100);
        }
        
        private void GameResume()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
