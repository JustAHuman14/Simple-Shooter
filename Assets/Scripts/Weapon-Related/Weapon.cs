using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using Assets.Scripts.UI;

namespace Assets.Scripts.Weapon_Related
{
    public class Weapon : MonoBehaviour, IPickable
    {
        [Header("Serialized Fields")]
        [SerializeField] public Bullet _bulletPrefab;
        [SerializeField] private GameObject _muzzleFlash;
        public WeaponSO weapon;

        [Header("Non-Serialized Fields")]
        private GameInput _gameInput;
        private Vector3 _bulletDirection;
        public int maxBulletsInMag;
        public int bulletsRemainingInMag;
        private IObjectPool<Bullet> _bulletObjectPool;
        private readonly int defaultCapacity = 500;
        private readonly int maxSize = 2000;
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        [SerializeField] private Transform _bulletSpawn;
        private AudioSource _audioSource;
        private ParticleSystem _muzzleFlashEffect;
        private bool _isShooting;
        public event Action<Weapon> OnShoot;
        public event Action<Weapon> OnReload;
        private bool _isPicked;

        public void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _muzzleFlashEffect = _muzzleFlash.GetComponent<ParticleSystem>();
            _bulletObjectPool = new ObjectPool<Bullet>(
                CreateBullet,
                bullet =>
                {
                    if (bullet != null)
                        bullet.gameObject.SetActive(true);
                },
                bullet => bullet.gameObject.SetActive(false),
                bullet => Destroy(bullet.gameObject),
                true,
                defaultCapacity,
                maxSize
            );
            maxBulletsInMag = weapon.maxBulletsInMag;
            bulletsRemainingInMag = maxBulletsInMag;
        }

	public void Start()
	{
	    _gameInput = GlobalReferences.Instance.gameInput;
	}

        private Bullet CreateBullet()
        {
            Bullet bulletInstance = Instantiate(_bulletPrefab);
            bulletInstance.bulletObjectPool = _bulletObjectPool;
            return bulletInstance;
        }

        protected virtual void Update()
        {
            _isPicked = transform.parent != null;

            if (_isPicked)
            {
            	Camera camera = GetComponentInParent<Player>().GetComponentInChildren<Camera>();
                Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
                Vector3 aimPoint = Physics.Raycast(ray, out RaycastHit hit, 1000f) ? hit.point : ray.GetPoint(1000f);
                _bulletDirection = (aimPoint - transform.position).normalized;

                HandleShootingAndReload();
            }
        }

        protected void HandleShootingAndReload()
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
            Bullet bulletObject = _bulletObjectPool.Get();

            bulletObject ??= CreateBullet();

            bulletObject.transform.position = _bulletSpawn.position;
            bulletObject.transform.rotation = Quaternion.LookRotation(_bulletDirection);

            if (bulletObject.TryGetComponent(out Rigidbody bulletRb))
                bulletRb.velocity = _bulletDirection * weapon.bulletSpeed;

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
        }

        private void OnDisable()
        {
            _reloadCoroutine = null;
            _shootCoroutine = null;
            _isShooting = false;
        }
    }
}
