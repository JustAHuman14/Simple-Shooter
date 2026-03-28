using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using Unity.Netcode;

namespace Assets.Scripts.Weapon_Related
{
    public class Weapon : NetworkBehaviour, IPickable
    {
        [Header("Serialized Fields")]
        [SerializeField] private GameObject _muzzleFlash;
        [SerializeField] private LayerMask _shootLayerMask;

        [Header("Non-Serialized Fields")]
        private GameInput _gameInput;
        private Vector3 _bulletDirection;
        public int maxBulletsInMag;
        public int bulletsRemainingInMag;
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        private AudioSource _audioSource;
        private ParticleSystem _muzzleFlashEffect;
        private bool _isShooting;
        public event Action<Weapon> OnShoot;
        public event Action<Weapon> OnReload;
        private bool _isPicked;
        private Rigidbody _rb;
        private Collider _collider;
        private Rigidbody _playerRb;
        private Camera _playerCamera;
        private float _dropForce;
        private bool _isPlayerQuitting;
        public WeaponSO weapon;
        Transform[] _children;

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
        }

        private void Update()
        {
            if (!IsLocalPlayer) return;
            if (_isPicked && _gameInput.IsPlayerDroppingWeapon())
            {
                transform.localRotation = Quaternion.Euler(10, 90, 0);
                transform.parent = null;
                _isPicked = false;
                _rb.isKinematic = false;
                _collider.isTrigger = false;
                _rb.velocity = _playerRb.velocity;
                _rb.AddForce(_bulletDirection * _dropForce, ForceMode.Impulse);
                gameObject.layer = default;

                foreach (Transform child in _children)
                {
                    child.gameObject.layer = 0;
                }
            }

            if (_isPicked)
            {
                foreach (Transform child in _children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Weapon");
                }

                Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
                Vector3 aimPoint = Physics.Raycast(ray, out RaycastHit hit, 1000f) ? hit.point : ray.GetPoint(1000f);
                _bulletDirection = (aimPoint - transform.position).normalized;
                HandleShootingAndReload();
            }
        }

        private void HandleShootingAndReload()
        {
            if (_gameInput.IsPlayerAttacking() && _reloadCoroutine == null && !_isShooting && bulletsRemainingInMag > 0 && !EventSystem.current.IsPointerOverGameObject())
                _shootCoroutine ??= StartCoroutine(ShootRoutine());

            if (_gameInput.IsPlayerReloading() && bulletsRemainingInMag < weapon.maxBulletsInMag && !_isShooting)
                _reloadCoroutine ??= StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ShootRoutine()
        {
            _isShooting = true;
            FireOneBullet();

            yield return new WaitForSeconds(weapon.secondsGapBetweenBullets);

            while (_gameInput.IsPlayerAttacking() && bulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(weapon.secondsGapBetweenBullets);
            }

            _shootCoroutine = null;
            _isShooting = false;
        }

        private void FireOneBullet()
        {
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, weapon.bulletRange))
            {
                print(hit.collider.gameObject.name);
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    damageable.Damage(hit);
                }

                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
        }

        private void OnDisable()
        {
            _reloadCoroutine = null;
            _shootCoroutine = null;
            _isShooting = false;
        }
    }
}
