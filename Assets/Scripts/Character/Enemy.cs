using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.Weapon_Related;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Character
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        //Serialized Fields
        [SerializeField] private MeshRenderer _enemyHeadMeshRenderer;
        [SerializeField] private MeshRenderer _enemyTorsoMeshRenderer;
        [SerializeField] private LayerMask _playerLayerMask;
        [SerializeField] private float _sightRange;
        [SerializeField] private float _attackRange;
        [SerializeField] private GameObject _bloodSprayEffectPrefab;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private Transform _camera;
        [SerializeField] private GameObject _muzzleFlash;

        //Non-Serialized Fields
        public float maxHealth = 200f;
        public float currentHealth;
        public event Action OnDamage;
        private Coroutine _attackCoroutine;
        private NavMeshAgent _agent;
        private bool _isPlayerInSightRange;
        private bool _isPlayerInAttackRange;
        private Transform _player;
        private float _fieldOfView = 60f;
        private readonly float _colorChangedAfterDamageSeconds = 0.05f;
        private AudioSource _weaponAudio;
        private ParticleSystem _muzzleFlashEffect;
        private Coroutine _reloadCoroutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            _muzzleFlashEffect = _muzzleFlash.GetComponent<ParticleSystem>();
            _weaponAudio = _weapon.GetComponent<AudioSource>();
        }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.Find(nameof(Player)).transform;
        }

        private void Update()
        {
            Vector3 playerDir = (_player.position - transform.position).normalized;
            float angleBetweenPlayerAndEnemy = Vector3.Angle(transform.forward, playerDir);
            float distanceBetweenPlayerAndEnemy = Vector3.Distance(transform.position, _player.position);

            _isPlayerInSightRange = distanceBetweenPlayerAndEnemy < _sightRange && angleBetweenPlayerAndEnemy > -_fieldOfView && angleBetweenPlayerAndEnemy < _fieldOfView;
            _isPlayerInAttackRange = angleBetweenPlayerAndEnemy > -_fieldOfView && angleBetweenPlayerAndEnemy < _fieldOfView && distanceBetweenPlayerAndEnemy < _attackRange;


            if (_isPlayerInSightRange && !_isPlayerInAttackRange)
                ChasePlayer();
            else if (_isPlayerInAttackRange)
                _attackCoroutine ??= StartCoroutine(AttackPlayer());

            if (_weapon.bulletsRemainingInMag == 0)
                _reloadCoroutine ??= StartCoroutine(ReloadWeapon());

        }

        private IEnumerator ReloadWeapon()
        {
            print("Enemy Reloading Weapon...");

            while (_weapon.bulletsRemainingInMag < _weapon.maxBulletsInMag)
            {
                _weapon.bulletsRemainingInMag++;
                yield return new WaitForSeconds(_weapon.weapon.secondsGapInReloading);
            }

            print("Enemy Weapon Reloaded...");

            _reloadCoroutine = null;
        }

        private IEnumerator AttackPlayer()
        {
            _agent?.SetDestination(transform.position);
            _agent?.transform.LookAt(_player);
            while (_weapon.bulletsRemainingInMag > 0 && _isPlayerInAttackRange && _reloadCoroutine == null)
            {
                AttackRoutine();
                yield return new WaitForSeconds(_weapon.weapon.secondsGapBetweenBullets);
            }

            _attackCoroutine = null;
        }

        private void AttackRoutine()
        {
            _agent?.SetDestination(transform.position);
            _agent?.transform.LookAt(_player);
            Vector3 bulletDir = _camera.transform.forward +
            (_camera.transform.right * Random.Range(-0.02f, 0.02f)) +
            (_camera.transform.up * Random.Range(0, 0.02f));

            if (Physics.Raycast(_camera.transform.position, bulletDir, out RaycastHit hit, _attackRange))
            {
                IDamageable damagebale = hit.collider.GetComponentInParent<IDamageable>();
                _weapon.bulletsRemainingInMag--;
                _weaponAudio.PlayOneShot(_weaponAudio.clip);
                _muzzleFlashEffect.Play();
                damagebale?.Damage(hit);
                print($"Enemy bullets: {_weapon.bulletsRemainingInMag}");
            }
        }

        public void Damage(RaycastHit hit)
        {
            switch (hit.collider.name)
            {
                case "EnemyHead":
                    TakeDamage(20, _enemyHeadMeshRenderer);
                    break;
                case "EnemyTorso":
                    TakeDamage(10, _enemyTorsoMeshRenderer);
                    break;
            }
        }

        private void ChasePlayer() => _agent?.SetDestination(_player.position);

        private void TakeDamage(int damage, MeshRenderer _enemyMeshRenderer)
        {
            currentHealth -= damage;
            OnDamage?.Invoke();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Destroy(gameObject);
                _agent = null;
                return;
            }

            StartCoroutine(PlayerDamagedRoutine(_enemyMeshRenderer));
        }

        private IEnumerator PlayerDamagedRoutine(MeshRenderer _enemyMeshRenderer)
        {
            _enemyMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(_colorChangedAfterDamageSeconds);
            _enemyMeshRenderer.material.color = Color.white;
        }
    }
}
