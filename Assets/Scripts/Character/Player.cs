using UnityEngine;
using System;
using TMPro;
using Assets.Scripts.Interfaces;
using UnityEngine.InputSystem;
using Assets.Scripts.Weapon_Related;
using System.Diagnostics;

namespace Assets.Scripts.Character
{
    public class Player : MonoBehaviour, ICharacter
    {
        // Serialized Fields
        [SerializeField] private GameInput _gameInput;
        [SerializeField] private LayerMask _groundLayerMask, _interactableLayerMask;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _gun, _gunSlot;
        [SerializeField] private float _groundValue = 1f, _groundRadius = 0.4f, _interactRadius = 2f;
        [SerializeField] private TextMeshProUGUI _interactTM;
        [SerializeField] private Transform _primaryWeaponSlot1, _primaryWeaponSlot2, _secondaryWeaponSlot;
        [SerializeField] private float _playerSpeed;

        // Non-Serialized Fields
        private Rigidbody _rb;
        private bool _isGrounded = true, _isJumping;
        private Vector3 _moveDirection;
        private readonly float _jumpForce = 5f;
        private bool _isSprinting;
        public event Action OnWeaponSwitch;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _gameInput.OnSprint += HandleSprint;
            _gameInput.OnWeaponSwitch += HandleActiveGun;

            _gameInput.OnJump += ctx => _isJumping = _isGrounded;
        }

        private void Update()
        {
            HandleSpeedAndDirection();
            HandleInteraction();
            HandleItemDrop();

            if (_gameInput.IsPlayerSprinting())
                _isSprinting = _isSprinting != true;
        }

        private void FixedUpdate()
        {
            CheckIsGrounded();
            PlayerMovement();
        }

        private void HandleSprint(InputAction.CallbackContext context)
        {
            _isSprinting = _isSprinting != true;
        }

        private void HandleItemDrop()
        {

        }

        private void HandleInteraction()
        {
            Debug.DrawRay(
            _playerCamera.transform.position,
            _playerCamera.transform.forward * _interactRadius,
            Color.red
            );
            if (Physics.Raycast(
                _playerCamera.transform.position,
                _playerCamera.transform.forward,
                out RaycastHit hit,
                _interactRadius
            ))
            {
                if (hit.collider.TryGetComponent(out IPickable pickable))
                {
                    print($"Press \"E\" to Interact with {hit.collider.gameObject.name}");

                    if (_gameInput.IsPlayerPicking())
                        if (hit.collider.TryGetComponent(out Weapon weapon))
                        {
                            if (weapon.weapon.weaponType == WeaponType.Primary)
                            {
                                if (!_primaryWeaponSlot1.childCount == 1)
                                    pickable.Pick(_primaryWeaponSlot1, WeaponType.Primary);
                                else if (!_primaryWeaponSlot2.childCount == 1)
                                    pickable.Pick(_primaryWeaponSlot2, WeaponType.Primary);
                                else
                                    print("You can only have 2 primary weapons!");

                            }
                            else if (weapon.weapon.weaponType == WeaponType.Secondary)
                            {
                                if (!_secondaryWeaponSlot.childCount == 1)
                                    pickable.Pick(_secondaryWeapon, WeaponType.Secondary);
                                else
                                    print("You can only have 1 secondary weapon!");
                            }
                        }
                }
            }
        }

        private void HandleActiveGun(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1)
            {
                if (_secondaryWeapon.activeInHierarchy)
                    _secondaryWeapon.SetActive(false);

                _primaryWeapon.SetActive(true);
                OnWeaponSwitch?.Invoke();
            }
            else if (ctx.ReadValue<float>() == 2)
            {
                if (_primaryWeapon.activeInHierarchy)
                    _primaryWeapon.SetActive(false);

                _secondaryWeapon.SetActive(true);
                OnWeaponSwitch?.Invoke();
            }
        }

        private void HandleSpeedAndDirection()
        {
            if (_gameInput.IsPlayerSprinting())
                _isSprinting = _isSprinting != true;

            _playerSpeed = _isSprinting ? 10 : 6;
            _moveDirection = _gameInput.GetPlayerMovementVector();
            _moveDirection = transform.forward * _moveDirection.y + transform.right * _moveDirection.x;
        }

        private void CheckIsGrounded()
        {
            Vector3 spherePosition = transform.position + Vector3.down * _groundValue;
            _isGrounded = Physics.CheckSphere(spherePosition, _groundRadius, _groundLayerMask);
        }

        private void PlayerMovement()
        {
            Vector3 targetVelocity = _moveDirection * _playerSpeed;
            _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);

            if (_isJumping)
            {
                _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
                _isJumping = false;
            }
        }

        public bool IsWalking() => _moveDirection != Vector3.zero;
    }
}
