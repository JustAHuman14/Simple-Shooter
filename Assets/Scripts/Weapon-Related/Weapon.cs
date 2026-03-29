using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using UnityEngine.InputSystem;
using Assets.Scripts.UI;

namespace Assets.Scripts.Weapon_Related
{
    public class Weapon : MonoBehaviour, IPickable
    {
        [Header("Serialized Fields")]
        [SerializeField] private GameObject _muzzleFlash;
        [SerializeField] private LayerMask _shootLayerMask;

        [Header("Non-Serialized Fields")]
        private float _dropForce;
        private GameInput _gameInput;
        private Vector3 _bulletDirection;
        public int maxBulletsInMag;
        public int bulletsRemainingInMag;
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        private AudioSource _audioSource;
        private ParticleSystem _muzzleFlashEffect;
        public event Action<Weapon> OnShoot;
        public event Action<Weapon> OnReload;
        private bool _isPicked;
        private Rigidbody _rb;
        private Collider _collider;
        private Rigidbody _playerRb;
        private Camera _playerCamera;
        public WeaponSO weapon;
        private Transform[] _children;
        private bool _isPlayerTryingToQuit;
        private GameObject _pauseMenuUI;

        private void Awake()
        {
            _children = gameObject.GetComponentsInChildren<Transform>(true);
            _dropForce = 3;
            _playerRb = GameObject.Find(nameof(Player)).GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();
            _muzzleFlashEffect = _muzzleFlash.GetComponent<ParticleSystem>();
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            maxBulletsInMag = weapon.maxBulletsInMag;
            bulletsRemainingInMag = maxBulletsInMag;
        }

        private void Start()
        {
            _gameInput = GlobalReferences.Instance.gameInput;
            _pauseMenuUI = GlobalReferences.Instance.pauseMenuUI;
            _gameInput.OnExit += OnExit;
        }

        private void OnExit(InputAction.CallbackContext context) => StopCoroutines();

        private void Update()
        {
            if (_isPicked) HandleShootingAndReload();

            HandleWeaponDrop();
        }

        private void HandleWeaponDrop()
        {
            if (_isPicked && _gameInput.IsPlayerDroppingWeapon())
            {
                StopCoroutines();
                transform.localRotation = Quaternion.Euler(10, 90, 0);
                transform.parent = null;
                _isPicked = false;
                _rb.isKinematic = false;
                _collider.isTrigger = false;
                _rb.velocity = _playerRb.velocity;
                _rb.AddForce(_playerCamera.transform.forward * _dropForce, ForceMode.Impulse);

                foreach (Transform child in _children)
                {
                    child.gameObject.layer = 0;
                }
            }
        }

        private void StopCoroutines()
        {
            StopAllCoroutines();
            _shootCoroutine = null;
            _reloadCoroutine = null;
        }

        private void HandleShootingAndReload()
        {
            _isPlayerTryingToQuit = _pauseMenuUI.activeInHierarchy;
            if (_gameInput.IsPlayerAttacking() && _reloadCoroutine == null && bulletsRemainingInMag > 0 && !_isPlayerTryingToQuit)
                _shootCoroutine ??= StartCoroutine(ShootRoutine());

            if (_gameInput.IsPlayerReloading() && bulletsRemainingInMag < weapon.maxBulletsInMag && _shootCoroutine == null && !_isPlayerTryingToQuit)
                _reloadCoroutine ??= StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ShootRoutine()
        {
            FireOneBullet();

            yield return new WaitForSeconds(weapon.secondsGapBetweenBullets);

            while (_gameInput.IsPlayerAttacking() && bulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(weapon.secondsGapBetweenBullets);
            }

            _shootCoroutine = null;
        }

        private void FireOneBullet()
        {
            float spreadDensityX = _gameInput.IsPlayerAiming() ? 0.01f : weapon.spreadDensityX;
            float spreadDensityY = _gameInput.IsPlayerAiming() ? 0.01f : weapon.spreadDensityY;

            Vector3 bulletDir = _playerCamera.transform.forward +
            (_playerCamera.transform.right * Random.Range(-spreadDensityX, spreadDensityX)) +
            (_playerCamera.transform.up * Random.Range(0, spreadDensityY));

            if (Physics.Raycast(_playerCamera.transform.position, bulletDir, out RaycastHit hit, weapon.bulletRange))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

                damageable?.Damage(hit);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Transform bulletImpactInstance = Instantiate(
                        GlobalReferences.Instance.bulletImpactPrefab.transform,
                        hit.point + (hit.normal * 0.01f),
                        Quaternion.LookRotation(hit.normal)
                    );

                    bulletImpactInstance.SetParent(hit.transform);

                }
            }

            bulletsRemainingInMag--;
            OnShoot?.Invoke(this);
            PlayEffectsAfterFiring();
        }

        private IEnumerator ReloadRoutine()
        {
            while (bulletsRemainingInMag < weapon.maxBulletsInMag)
            {
                bulletsRemainingInMag++;
                OnReload?.Invoke(this);
                yield return new WaitForSeconds(weapon.secondsGapInReloading);
            }

            _reloadCoroutine = null;
        }

        private void PlayEffectsAfterFiring()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
            _muzzleFlashEffect.Play();
        }

        public void Pick(Transform weaponSlot)
        {
            transform.SetParent(weaponSlot);
            transform.localPosition = weapon.gunPosition;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            _isPicked = true;
            _rb.isKinematic = true;
            _collider.isTrigger = true;
            _playerCamera = GetComponentInParent<Player>().GetComponentInChildren<Camera>();

            foreach (Transform child in _children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }

        private void OnDisable()
        {
            StopCoroutines();
        }

        private void OnDestroy()
        {
            if (_gameInput != null)
                _gameInput.OnExit -= OnExit;
        }
    }
}
