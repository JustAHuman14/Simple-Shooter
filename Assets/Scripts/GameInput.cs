using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Assets.Scripts
{
    public class GameInput : MonoBehaviour
    {
        // Private Fields
        private PlayerInputActions _playerInput;
        private Vector2 _moveDirection;
        private Vector2 _playerHeadMovement;
        private Vector2 _playerPointerPosition;

        // Public Fields
        public event Action<InputAction.CallbackContext> OnWeaponSwitch;
        public event Action<InputAction.CallbackContext> OnSprint;
        public event Action<InputAction.CallbackContext> OnJump;
        
        private void Awake() => _playerInput = new PlayerInputActions();

        private void Start()
        {
            _playerInput.Player.WeaponSwitch.performed += PlayerInput_OnWeaponSwitch;
            _playerInput.Player.Sprint.performed += PlayerInput_OnSprint;
            _playerInput.Player.Jump.performed += PlayerInput_OnJump;
        }

        private void OnEnable() => _playerInput.Player.Enable();
        private void OnDisable() => _playerInput.Player.Disable();

        private void PlayerInput_OnWeaponSwitch(InputAction.CallbackContext context) => OnWeaponSwitch?.Invoke(context);
        private void PlayerInput_OnSprint(InputAction.CallbackContext context) => OnSprint?.Invoke(context);
        private void PlayerInput_OnJump(InputAction.CallbackContext context) => OnJump?.Invoke(context);

        public bool IsPlayerAttacking() => _playerInput.Player.Attack.IsPressed();
        public bool IsPlayerJumping() => _playerInput.Player.Jump.IsPressed();
        public bool IsPlayerSprinting() => _playerInput.Player.Sprint.IsPressed();
	public bool IsPlayerReloading() => _playerInput.Player.Reload.IsPressed();
	public bool IsPlayerPicking() => _playerInput.Player.Pickup.IsPressed();
	public bool IsPlayerDropping() => _playerInput.Player.Drop.IsPressed();
	
        public Vector2 GetPlayerMovementVector() => _moveDirection = _playerInput.Player.Move.ReadValue<Vector2>();
        public Vector2 GetPlayerHeadMovement() => _playerHeadMovement = _playerInput.Player.HeadRotate.ReadValue<Vector2>();
        public Vector2 GetPlayerPointerPosition() => _playerPointerPosition = _playerInput.Player.PointerPosition.ReadValue<Vector2>();
    }
}
