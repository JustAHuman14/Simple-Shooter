using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        //Serialized Fields
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _playerHead;
        [SerializeField] private float _mouseSensitivity;
        [SerializeField] private Camera _gunCamera;

        //Non-Serialized Fields
        private GameInput _gameInput;
        private Camera _playerPOV;
        private float _xRotation;
        private float _mouseX;
        private float _mouseY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _gameInput = GlobalReferences.Instance.gameInput;
            _mouseSensitivity = GameManager.mouseSensitivity;
            _playerPOV = GetComponent<Camera>();
        }

        private void Update()
        {
            _playerPOV.fieldOfView = _gameInput.IsPlayerAiming() ? 30 : 60;
            _gunCamera.fieldOfView = _gameInput.IsPlayerAiming() ? 30 : 60;
            _mouseSensitivity = GameManager.mouseSensitivity;
            HandleRotation();

            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -50, 50);

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
