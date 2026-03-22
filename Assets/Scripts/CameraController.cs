using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        //Serialized Fields
        [Header("Player Related Settings")]
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _playerHead;

        [Header("Mouse Related Settings")]
        [SerializeField] private float _mouseSensitivity;

        //Non-Serialized Fields
        private GameInput _gameInput;
        private float _xRotation;
        private float _mouseX;
        private float _mouseY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _gameInput = GlobalReferences.Instance.gameInput;
            _mouseSensitivity = GameManager.mouseSensitivity;
            _gameInput.OnExit += ctx => Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            _mouseSensitivity = GameManager.mouseSensitivity;
            HandleRotation();

            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -65, 40);

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
