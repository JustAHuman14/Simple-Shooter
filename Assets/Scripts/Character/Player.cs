using UnityEngine;
using System;
using TMPro;
using Assets.Scripts.Interfaces;
using UnityEngine.InputSystem;
using Assets.Scripts.Weapon_Related;

namespace Assets.Scripts.Character
{
    public class Player : MonoBehaviour, ICharacter
    {
        // Serialized Fields
        [SerializeField] private LayerMask _groundLayerMask, _interactableLayerMask;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _groundValue = 1f, _groundRadius = 0.4f, _interactRadius = 2f;
        [SerializeField] private Transform _primaryWeaponSlot1, _primaryWeaponSlot2, _secondaryWeaponSlot;
        [SerializeField] private float _playerSpeed;
        [SerializeField] private Enemy _enemy;

        // Non-Serialized Fields
        private GameInput _gameInput;
        private Rigidbody _rb;
        private bool _isGrounded = true, _isJumping, _isSprinting, _isGunInPickingRange;
        private Vector3 _moveDirection;
        private readonly float _jumpForce = 5f;
        private Weapon _weapon, primaryWeapon1, primaryWeapon2, secondaryWeapon;
        private GameObject _pickupUI;
        private bool _isPickingWeapon;

        public event Action<Weapon> OnWeaponSwitch, OnWeaponShoot, OnWeaponReload;
        public event Action<GameObject> OnGunInPickingRange;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _pickupUI = GameObject.Find("PickupUI");
        }

        private void Start()
        {
            _gameInput = GlobalReferences.Instance.gameInput;
            _gameInput.OnSprint += HandleSprint;
            _gameInput.OnWeaponSwitch += HandleActiveGun;
            _gameInput.OnJump += ctx => _isJumping = _isGrounded;
            _gameInput.OnSpawnEnemy += SpawnEnemy;
            _gameInput.OnWeaponPick += ctx => _isPickingWeapon = true;
        }

        private void SpawnEnemy(InputAction.CallbackContext context)
        {
            Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, _groundLayerMask))
            {
                Vector3 spawnPoint = hit.point;
                Enemy enemy = Instantiate(_enemy, spawnPoint, Quaternion.identity);
                enemy.transform.LookAt(transform);
            }
        }

        private void Update()
        {
            HandleSpeedAndDirection();
            HandleInteraction();

            _pickupUI.SetActive(_isGunInPickingRange);
        }

        private void FixedUpdate()
        {
            CheckIsGrounded();
            PlayerMovement();
        }

        private void HandleSprint(InputAction.CallbackContext context) => _isSprinting = _isSprinting != true;

        private void HandleInteraction()
        {
            if (Physics.Raycast(
                _playerCamera.transform.position,
                _playerCamera.transform.forward,
                out RaycastHit hit,
                _interactRadius))
            {
                if (hit.collider.TryGetComponent(out IPickable pickable))
                {
                    print($"Press \"E\" to Interact with {hit.collider.gameObject.name}");
                    OnGunInPickingRange?.Invoke(hit.collider.gameObject);
                    _isGunInPickingRange = true;

                    if (_isPickingWeapon)
                    {
                        if (hit.collider.TryGetComponent(out Weapon weapon))
                        {
                            weapon.OnShoot += weapon => OnWeaponShoot?.Invoke(weapon);
                            weapon.OnReload += weapon => OnWeaponReload?.Invoke(weapon);

                            if (weapon.weapon.weaponType == WeaponType.Primary)
                            {
                                if (_primaryWeaponSlot1.childCount == 0)
                                {
                                    pickable.Pick(_primaryWeaponSlot1);
                                    primaryWeapon1 = weapon;
                                    WeaponSwitch(1, primaryWeapon1);
                                    _isPickingWeapon = false;
                                }
                                else if (_primaryWeaponSlot1.childCount == 1)
                                {
                                    pickable.Pick(_primaryWeaponSlot2);
                                    primaryWeapon2 = weapon;
                                    WeaponSwitch(2, primaryWeapon2);
                                    _isPickingWeapon = false;
                                }
                                else
                                    print("You can only have 2 primary weapons!");
                            }
                            else if (weapon.weapon.weaponType == WeaponType.Secondary)
                            {
                                if (_secondaryWeaponSlot.childCount == 1)
                                {
                                    print("You can only have 1 secondary weapon!");
                                    return;
                                }

                                pickable.Pick(_secondaryWeaponSlot);
                                secondaryWeapon = weapon;
                                WeaponSwitch(3, secondaryWeapon);
                                _isPickingWeapon = false;
                            }
                        }
                    }
                }
                else
                {
                    _isGunInPickingRange = false;
                    _isPickingWeapon = false;
                }
            }
            else
            {
                _isGunInPickingRange = false;
                _isPickingWeapon = false;
            }
        }

        private void HandleActiveGun(InputAction.CallbackContext ctx)
        {
            float weaponNum = ctx.ReadValue<float>();

            if (weaponNum == 1 && primaryWeapon1 != null)
                WeaponSwitch(weaponNum, primaryWeapon1);
            else if (weaponNum == 2 && primaryWeapon2 != null)
                WeaponSwitch(weaponNum, primaryWeapon2);
            else if (weaponNum == 3 && secondaryWeapon != null)
                WeaponSwitch(weaponNum, secondaryWeapon);
        }

        private void WeaponSwitch(float weaponNum, Weapon weapon)
        {
            _primaryWeaponSlot1.gameObject.SetActive(weaponNum == 1);
            _primaryWeaponSlot2.gameObject.SetActive(weaponNum == 2);
            _secondaryWeaponSlot.gameObject.SetActive(weaponNum == 3);

            OnWeaponSwitch?.Invoke(weapon);
        }

        private void HandleSpeedAndDirection()
        {
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
