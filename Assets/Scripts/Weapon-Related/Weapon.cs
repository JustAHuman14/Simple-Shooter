using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts
{
    public abstract class Weapon : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] public Bullet _bulletPrefab;
        [SerializeField] public Transform _bulletSpawn;
        [SerializeField] public Camera _camera;
        [SerializeField] public Animator _animator;
        [SerializeField] public GameObject _muzzleFlash;

        [Header("Non-Serialized Fields")]
        private AudioSource _audioSource;
        private ParticleSystem _muzzleFlashEffect;
        private Vector3 _bulletDirection;
        public int maxNumOfBulletsInMag = 30;
        public int bulletsRemainingInMag;
        private IObjectPool<Bullet> _bulletObjectPool;
        private int defaultCapacity = 120;
        private int maxSize = 500;
        protected abstract float SecondsGapBetweenBullets { get; set; }
        private readonly float _secondsGapInReloading = 0.01f;
        protected abstract float BulletSpeed { get; set; }
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        private bool _isShooting;
        public event Action OnShoot;
        public event Action OnReload;

        public void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _muzzleFlashEffect = _muzzleFlash.GetComponent<ParticleSystem>();
            _bulletObjectPool = new ObjectPool<Bullet>(
                CreateBullet,
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledObject,
                true,
                defaultCapacity,
                maxSize
            );

            bulletsRemainingInMag = maxNumOfBulletsInMag;
        }

        private void OnDestroyPooledObject(Bullet bullet)
        {
            Destroy(bullet.gameObject);
        }

        private void OnGetFromPool(Bullet bullet)
        {
            if (bullet != null)
                bullet.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }

        private Bullet CreateBullet()
        {
            Bullet bulletInstance = Instantiate(_bulletPrefab);
            bulletInstance.objectPool = _bulletObjectPool;
            return bulletInstance;
        }

        protected virtual void Update()
        {
            Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            Vector3 aimPoint = Physics.Raycast(ray, out RaycastHit hit, 1000f) ? hit.point : ray.GetPoint(1000f);
            _bulletDirection = (aimPoint - transform.position).normalized;

            HandleShootingAndReload();
        }

        protected void HandleShootingAndReload()
        {
            if (Input.GetMouseButtonDown(0) && !_isShooting && bulletsRemainingInMag > 0)
            {
                if (_reloadCoroutine != null)
                {
                    StopCoroutine(_reloadCoroutine);
                    _reloadCoroutine = null;
                }

                _shootCoroutine ??= StartCoroutine(ShootRoutine());
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsRemainingInMag < maxNumOfBulletsInMag && !_isShooting)
            {
                _reloadCoroutine ??= StartCoroutine(ReloadRoutine());
            }
        }

        private IEnumerator ShootRoutine()
        {
            _isShooting = true;
            FireOneBullet();

            yield return new WaitForSeconds(SecondsGapBetweenBullets);

            while (Input.GetMouseButton(0) && bulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(SecondsGapBetweenBullets);
            }

            _isShooting = false;
        }

        private void FireOneBullet()
        {
            Bullet bulletObject = _bulletObjectPool.Get();

            bulletObject ??= CreateBullet();

            bulletObject.transform.position = _bulletSpawn.position;
            bulletObject.transform.rotation = Quaternion.LookRotation(_bulletDirection);

            if (bulletObject.TryGetComponent(out Rigidbody bulletRb))
                bulletRb.velocity = _bulletDirection * BulletSpeed;

            bulletsRemainingInMag--;
            OnShoot?.Invoke();
            PlayEffectsAfterFiring();
        }

        private IEnumerator ReloadRoutine()
        {
            while (bulletsRemainingInMag < maxNumOfBulletsInMag)
            {
                bulletsRemainingInMag++;
                OnReload?.Invoke();
                yield return new WaitForSeconds(_secondsGapInReloading);
            }

            _reloadCoroutine = null;
        }

        private void PlayEffectsAfterFiring()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
            _muzzleFlashEffect.Play();
        }
    }
}

