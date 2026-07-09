using UnityEngine;
using System;
using TMPro;
using Assets.Scripts.Interfaces;
using UnityEngine.InputSystem;
using Assets.Scripts.Weapon_Related;
using System.Collections;
using JetBrains.Annotations;

namespace Assets.Scripts.Character
{
    public class Player : MonoBehaviour, IDamageable
    {
        // Serialized Fields
#pragma warning disable CS0649
        [SerializeField] private LayerMask _groundLayerMask, _interactableLayerMask;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _primaryWeaponSlot1;
        [SerializeField] private Transform _primaryWeaponSlot2;
        [SerializeField] private Transform _secondaryWeaponSlot;
        [SerializeField] private Transform _throwablesSlot;
        [SerializeField] private float _playerSpeed;
        [SerializeField] private Enemy _enemy;
        [SerializeField] private MeshRenderer _playerHeadMeshRenderer, _playerTorsoMeshRenderer;
        [SerializeField] private TextMeshProUGUI _fpsText;
#pragma warning restore CS0649

        [SerializeField] private readonly float _groundValue = 1f;
        [SerializeField] private readonly float _groundRadius = 0.4f;
        [SerializeField] private readonly float _interactRadius = 2f;

        // Non-Serialized Fields
        private GameInput _gameInput;
        private Rigidbody _rb;
        private bool _isGrounded = true, _isJumping, _isSprinting, _isGunInPickingRange;
        private Vector3 _moveDirection;
        private readonly float _jumpForce = 5f;
        private IWeapon _weapon;
        private IWeapon _primaryWeapon1;
        private IWeapon _primaryWeapon2;
        private IWeapon _secondaryWeapon;
        private IWeapon _tertiaryWeapon;
        private GameObject _pickupUi;
        private bool _isPickingWeapon;
        private readonly float _colorChangedAfterDamageSeconds = 0.05f;
        private Coroutine _fpsCoroutine;
        public float MmaxHealth = 200f, CurrentHealth;
        private int _frameCount;
        public event Action<IWeapon> OnWeaponSwitch, OnWeaponShoot;
        public event Action<Weapon> OnWeaponReload;
        public event Action<GameObject> OnGunInPickingRange;
        public event Action OnDamage;

        [UsedImplicitly]
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            GameManager.mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
            CurrentHealth = MmaxHealth;
        }

        [UsedImplicitly]
        private void Start()
        {
            _pickupUi = GlobalReferences.Instance.pickupUI;
            _gameInput = GlobalReferences.Instance.gameInput;
            _gameInput.OnSprint += HandleSprint;
            _gameInput.OnWeaponSwitch += HandleActiveGun;
            _gameInput.OnJump += _ => _isJumping = _isGrounded;
            _gameInput.OnSpawnEnemy += SpawnEnemy;
            _gameInput.OnWeaponPick += _ => _isPickingWeapon = true;
            StartCoroutine(FpsRoutine());
        }

        private void SpawnEnemy(InputAction.CallbackContext context)
        {
            Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 60f, _groundLayerMask))
            {
                Vector3 spawnPoint = hit.point;
                Enemy enemy = Instantiate(_enemy, spawnPoint, Quaternion.identity);
                enemy.transform.LookAt(transform);
            }
        }

        [UsedImplicitly]
        private void Update()
        {
            _frameCount++;

            if (Time.timeScale != 0)
            {
                HandleSpeedAndDirection();
                HandleInteraction();
                _pickupUi.SetActive(_isGunInPickingRange);
            }
        }

        private IEnumerator FpsRoutine()
        {
            float interval = 0.4f;

            while (true)
            {
                yield return new WaitForSecondsRealtime(interval);

                int fps = (int)(_frameCount / interval);
                _fpsText.text = $"FPS {fps}";
                _frameCount = 0;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            CheckIsGrounded();
            PlayerMovement();
        }

        private void HandleSprint(InputAction.CallbackContext context) => _isSprinting = !_isSprinting;

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
                    if (pickable.IsPicked) return;

                    OnGunInPickingRange?.Invoke(hit.collider.gameObject);
                    _isGunInPickingRange = true;

                    if (_isPickingWeapon)
                    {
                        if (hit.collider.TryGetComponent(out Weapon weapon))
                        {
                            weapon.OnShoot += _ => OnWeaponShoot?.Invoke(weapon);
                            weapon.OnReload += _ => OnWeaponReload?.Invoke(weapon);

                            if (weapon.WeaponSo.weaponType == WeaponType.Primary)
                            {
                                switch (_primaryWeaponSlot1.childCount)
                                {
                                    case 0:
                                        pickable.Pick(_primaryWeaponSlot1);
                                        weapon.OnWeaponDropped += () => _primaryWeapon1 = null;
                                        _primaryWeapon1 = weapon;
                                        WeaponSwitch(1, _primaryWeapon1);
                                        _isPickingWeapon = false;
                                        break;
                                    case 1 when _primaryWeaponSlot2.childCount == 0:
                                        pickable.Pick(_primaryWeaponSlot2);
                                        _primaryWeapon2 = weapon;
                                        weapon.OnWeaponDropped += () => _primaryWeapon2 = null;
                                        WeaponSwitch(2, _primaryWeapon2);
                                        _isPickingWeapon = false;
                                        break;
                                    default:
                                    {
                                        if (_primaryWeaponSlot2.childCount == 1 && _primaryWeaponSlot1.childCount == 1)
                                        {
                                            print("You can only have 2 primary weapons!");
                                        }

                                        break;
                                    }
                                }
                            }
                            else if (weapon.WeaponSo.weaponType == WeaponType.Secondary)
                            {
                                if (_secondaryWeaponSlot.childCount == 1)
                                {
                                    print("You can only have 1 secondary weapon!");
                                    return;
                                }

                                pickable.Pick(_secondaryWeaponSlot);
                                _secondaryWeapon = weapon;
                                weapon.OnWeaponDropped += () => _secondaryWeapon = null;
                                WeaponSwitch(3, _secondaryWeapon);
                                _isPickingWeapon = false;
                            }
                        }
                        else if (hit.collider.TryGetComponent(out IThrowable throwable))
                        {
                            if (_throwablesSlot.childCount >= 1) return;

                            pickable.Pick(_throwablesSlot);
                            _tertiaryWeapon = throwable;
                            WeaponSwitch(4, _secondaryWeapon);
                            _isPickingWeapon = false;
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

            if ((int)weaponNum == 1 && _primaryWeapon1 != null)
                WeaponSwitch(weaponNum, _primaryWeapon1);
            else if ((int)weaponNum == 2 && _primaryWeapon2 != null)
                WeaponSwitch(weaponNum, _primaryWeapon2);
            else if ((int)weaponNum == 3 && _secondaryWeapon != null)
                WeaponSwitch(weaponNum, _secondaryWeapon);
            else if ((int)weaponNum == 4 && _tertiaryWeapon != null)
                WeaponSwitch(weaponNum, _tertiaryWeapon);
        }

        private void WeaponSwitch(float weaponNum, IWeapon weapon)
        {
            _primaryWeaponSlot1.gameObject.SetActive((int)weaponNum == 1);
            _primaryWeaponSlot2.gameObject.SetActive((int)weaponNum == 2);
            _secondaryWeaponSlot.gameObject.SetActive((int)weaponNum == 3);
            _throwablesSlot.gameObject.SetActive((int)weaponNum == 4);
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

            if (!_isJumping) return;
            _rb.AddForce(transform.up * _jumpForce, ForceMode.VelocityChange);
            _isJumping = false;
        }

        public void Damage(RaycastHit hit)
        {
            switch (hit.collider.name)
            {
                case "PlayerHead":
                    TakeDamage(20, _playerHeadMeshRenderer);
                    break;
                case "PlayerTorso":
                    TakeDamage(10, _playerTorsoMeshRenderer);
                    break;
            }
        }

        private void TakeDamage(int damage, MeshRenderer playerMeshRenderer)
        {
            CurrentHealth -= damage;
            OnDamage?.Invoke();

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Cursor.lockState = CursorLockMode.None;
                SceneChanger.Instance.LoadScene(0);
            }

            StartCoroutine(PlayerDamagedRoutine(playerMeshRenderer));
        }

        private IEnumerator PlayerDamagedRoutine(MeshRenderer playerMeshRenderer)
        {
            playerMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(_colorChangedAfterDamageSeconds);
            playerMeshRenderer.material.color = Color.yellow;
        }
    }
}
