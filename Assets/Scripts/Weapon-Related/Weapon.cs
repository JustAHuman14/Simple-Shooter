using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using JetBrains.Annotations;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Weapon_Related
{
    public class Weapon : MonoBehaviour, IPickable, IWeapon
    {
        [Header("Serialized Fields")]
#pragma warning disable CS0649
        [SerializeField] private ParticleSystem _muzzleFlashEffect;
        [SerializeField] private LayerMask _shootLayerMask;
        [SerializeField] private float _force;
#pragma warning restore CS0649

        [Header("Non-Serialized Fields")]
        private float _dropForce;
        private GameInput _gameInput;
        private Vector3 _bulletDirection;
        public int MaxBulletsInMag;
        public int BulletsRemainingInMag;
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        private AudioSource _audioSource;
        public event Action<Weapon> OnShoot;
        public event Action<Weapon> OnReload;
        public bool IsPicked { get; private set; }
        private Rigidbody _rb;
        private Collider _collider;
        private Rigidbody _playerRb;
        private Camera _playerCamera;
        public WeaponSO WeaponSo;
        private Transform[] _children;
        private bool _isPlayerTryingToQuit;
        private GameObject _pauseMenuUi;
        public event Action OnWeaponDropped;

        [UsedImplicitly]
        private void Awake()
        {
            _children = gameObject.GetComponentsInChildren<Transform>(true);
            _dropForce = 3;
            _playerRb = GameObject.Find(nameof(Player)).GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            MaxBulletsInMag = WeaponSo.maxBulletsInMag;
            BulletsRemainingInMag = MaxBulletsInMag;
        }

        [UsedImplicitly]
        private void Start()
        {
            _gameInput = GlobalReferences.Instance.gameInput;
            _pauseMenuUi = GlobalReferences.Instance.pauseMenuUI;
            _gameInput.OnExit += OnExit;
        }

        private void OnExit(InputAction.CallbackContext context) => StopCoroutines();

        [UsedImplicitly]
        private void Update()
        {
            if (IsPicked) HandleShootingAndReload();
            HandleWeaponDrop();
        }

        private void HandleWeaponDrop()
        {
            if (!IsPicked || !_gameInput.IsPlayerDroppingWeapon()) return;
            StopCoroutines();
            transform.localRotation = Quaternion.Euler(10, 90, 0);
            transform.parent = null;
            IsPicked = false;
            _rb.isKinematic = false;
            _collider.isTrigger = false;
            _rb.velocity = _playerRb.velocity;
            _rb.AddForce(_playerCamera.transform.forward * _dropForce, ForceMode.Impulse);

            foreach (Transform child in _children)
            {
                child.gameObject.layer = 0;
            }
            OnWeaponDropped?.Invoke();
        }

        private void StopCoroutines()
        {
            StopAllCoroutines();
            _shootCoroutine = null;
            _reloadCoroutine = null;
        }

        private void HandleShootingAndReload()
        {
            _isPlayerTryingToQuit = _pauseMenuUi.activeInHierarchy;
            if (_gameInput.IsPlayerAttacking() && _reloadCoroutine == null && BulletsRemainingInMag > 0 && !_isPlayerTryingToQuit)
                _shootCoroutine ??= StartCoroutine(ShootRoutine());

            if (_gameInput.IsPlayerReloading() && BulletsRemainingInMag < WeaponSo.maxBulletsInMag && _shootCoroutine == null && !_isPlayerTryingToQuit)
                _reloadCoroutine ??= StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ShootRoutine()
        {
            FireOneBullet();

            yield return new WaitForSeconds(WeaponSo.secondsGapBetweenBullets);

            while (_gameInput.IsPlayerAttacking() && BulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(WeaponSo.secondsGapBetweenBullets);
            }

            _shootCoroutine = null;
        }

        private void FireOneBullet()
        {
            float spreadDensityX = _gameInput.IsPlayerAiming() ? 0.01f : WeaponSo.spreadDensityX;
            float spreadDensityY = _gameInput.IsPlayerAiming() ? 0.01f : WeaponSo.spreadDensityY;

            _bulletDirection = _playerCamera.transform.forward +
            (_playerCamera.transform.right * Random.Range(-spreadDensityX, spreadDensityX)) +
            (_playerCamera.transform.up * Random.Range(0, spreadDensityY));

            if (Physics.Raycast(_playerCamera.transform.position, _bulletDirection, out RaycastHit hit, WeaponSo.bulletRange))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    Rigidbody ragdollRb = hit.collider.attachedRigidbody;
                    NPC npc = hit.collider.gameObject.GetComponentInParent<NPC>();
                    npc?.TurnToRagdoll(_collider);
                    Physics.SyncTransforms();
                    ragdollRb?.AddForceAtPosition(_bulletDirection * _force, hit.point, ForceMode.Impulse);
                }
                
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

            BulletsRemainingInMag--;
            OnShoot?.Invoke(this);
            PlayEffectsAfterFiring();
        }

        private IEnumerator ReloadRoutine()
        {
            while (BulletsRemainingInMag < WeaponSo.maxBulletsInMag)
            {
                BulletsRemainingInMag++;
                OnReload?.Invoke(this);
                yield return new WaitForSeconds(WeaponSo.secondsGapInReloading);
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
            if (IsPicked) return;

            transform.SetParent(weaponSlot);
            transform.localPosition = WeaponSo.gunPosition;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            IsPicked = true;
            _rb.isKinematic = true;
            _collider.isTrigger = true;
            _playerCamera = GetComponentInParent<Player>().GetComponentInChildren<Camera>();

            foreach (Transform child in _children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }

        [UsedImplicitly]
        private void OnDisable() => StopCoroutines();

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (_gameInput != null)
                _gameInput.OnExit -= OnExit;
        }
    }
}
