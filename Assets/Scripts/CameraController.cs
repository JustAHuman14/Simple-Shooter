using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        //Serialized Fields
        [Header("Player Related Settings")]
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _playerHead;

        [Header("Mouse Related Settings")]
        [SerializeField] private float _mouseSensitivity = 40f;

        [Header("Game Input")]
        [SerializeField] private GameInput _gameInput;

        //Non-Serialized Fields
        private float _xRotation;
        private float _mouseX;
        private float _mouseY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            Vector2 pointerPosition = _gameInput.GetPlayerPointerPosition();
          
            if (Keyboard.current.escapeKey.isPressed)
            {
                Cursor.lockState = CursorLockMode.None;
                ResetRotation();
            }    

            if (Mouse.current.leftButton.isPressed)
                Cursor.lockState = CursorLockMode.Locked;

	    if (pointerPosition.x >= Screen.width / 2 && Application.isMobilePlatform)
	    {             
                HandleRotation();
            }
            else if (!Application.isMobilePlatform && Cursor.lockState == CursorLockMode.Locked)
            {
            	HandleRotation();
            }
            else
            {
                ResetRotation();
            }
            
            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -65, 65);

            _playerHead.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            _player.Rotate(Vector3.up * _mouseX);
        }

	private void HandleRotation()
	{
	    Vector2 mouseDelta = _gameInput.GetPlayerHeadMovement();
            _mouseX = mouseDelta.x * _mouseSensitivity * Time.deltaTime;
            _mouseY = mouseDelta.y * _mouseSensitivity * Time.deltaTime;
        }

        private void ResetRotation()
        {
           _mouseX = 0;
           _mouseY = 0;
        }
	
        public bool IsRotating() => _mouseX != 0 || _mouseY != 0;
    }
}
