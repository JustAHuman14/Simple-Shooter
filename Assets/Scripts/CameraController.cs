using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        //Serialized Fields
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private Transform _player;
        [SerializeField] private Camera _playerPov;
        [SerializeField] private Transform _playerHead;
        [SerializeField] private float _mouseSensitivity;
        [SerializeField] private Camera _gunCamera;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        //Non-Serialized Fields
        private GameInput _gameInput;
        private float _xRotation;
        private float _mouseX;
        private float _mouseY;

        [UsedImplicitly]
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _gameInput = GlobalReferences.Instance.GameInput;
            _mouseSensitivity = GameManager.MouseSensitivity;
        }

        [UsedImplicitly]
        private void Update()
        {
            _playerPov.fieldOfView = _gameInput.IsPlayerAiming() ? 60 : 90;
            _gunCamera.fieldOfView = _gameInput.IsPlayerAiming() ? 50 : 80;
            _mouseSensitivity = GameManager.MouseSensitivity;
            HandleRotation();

            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -80, 50);

            _playerHead.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            _player.Rotate(Vector3.up * _mouseX);
        }

        private void HandleRotation()
        {
            Vector2 mouseDelta = _gameInput.GetPlayerHeadMovement();
            _mouseX = mouseDelta.x * _mouseSensitivity * Time.deltaTime;
            _mouseY = mouseDelta.y * _mouseSensitivity * Time.deltaTime;
        }
    }
}
