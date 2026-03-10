using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Weapon_Related
{
    public class Weapon : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] public Bullet _bulletPrefab;
        [SerializeField] public Camera _camera;
        [SerializeField] private WeaponSO _weapon;
        [SerializeField] private GameInput _gameInput;

        [Header("Non-Serialized Fields")]
        private Vector3 _bulletDirection;
        public int bulletsRemainingInMag;
        private IObjectPool<Bullet> _bulletObjectPool;
        private int defaultCapacity = 120;
        private int maxSize = 500;
        private Coroutine _shootCoroutine;
        private Coroutine _reloadCoroutine;
        private bool _isShooting;
        public event Action OnShoot;
        public event Action OnReload;

        public void Awake()
        {
            _bulletObjectPool = new ObjectPool<Bullet>(
                CreateBullet,
                OnGetFromPool,
                bullet => bullet.gameObject.SetActive(false),
                bullet => Destroy(bullet.gameObject),
                true,
                defaultCapacity,
                maxSize
            );

            bulletsRemainingInMag = _weapon.maxBulletsInMag;
        }

        private void OnGetFromPool(Bullet bullet)
        {
            if (bullet != null)
                bullet.gameObject.SetActive(true);
        }

        private Bullet CreateBullet()
        {
            Bullet bulletInstance = Instantiate(_bulletPrefab);
            bulletInstance.bulletObjectPool = _bulletObjectPool;
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
            if (_gameInput.IsPlayerAttacking() && _reloadCoroutine == null && !_isShooting && bulletsRemainingInMag > 0)
            {
                _shootCoroutine ??= StartCoroutine(ShootRoutine());
            }

            if (_gameInput.IsPlayerReloading() && bulletsRemainingInMag < _weapon.maxBulletsInMag && !_isShooting)
            {
                _reloadCoroutine ??= StartCoroutine(ReloadRoutine());
            }
        }

        private IEnumerator ShootRoutine()
        {
            _isShooting = true;
            FireOneBullet();

            yield return new WaitForSeconds(_weapon.secondsGapBetweenBullets);

            while (_gameInput.IsPlayerAttacking() && bulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(_weapon.secondsGapBetweenBullets);
            }

            _shootCoroutine = null;
            _isShooting = false;
        }

        private void FireOneBullet()
        {
            Bullet bulletObject = _bulletObjectPool.Get();

            bulletObject ??= CreateBullet();

            bulletObject.transform.position = _weapon.bulletSpawn.position;
            bulletObject.transform.rotation = Quaternion.LookRotation(_bulletDirection);

            if (bulletObject.TryGetComponent(out Rigidbody bulletRb))
                bulletRb.velocity = _bulletDirection * _weapon.bulletSpeed;

            bulletsRemainingInMag--;
            OnShoot?.Invoke();
            PlayEffectsAfterFiring();
        }

        private IEnumerator ReloadRoutine()
        {
            while (bulletsRemainingInMag < _weapon.maxBulletsInMag)
            {
                bulletsRemainingInMag++;
                OnReload?.Invoke();
                yield return new WaitForSeconds(_weapon.secondsGapInReloading);
            }

            _reloadCoroutine = null;
        }

        private void PlayEffectsAfterFiring()
        {
            _weapon.audioSource.PlayOneShot(_weapon.audioSource.clip);
            _weapon.muzzleFlashEffect.Play();
        }
    }
}
