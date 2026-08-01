using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.Weapon_Related;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Character
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        //Serialized Fields
#pragma warning disable CS0649 
        [SerializeField] private MeshRenderer _enemyHeadMeshRenderer;
        [SerializeField] private MeshRenderer _enemyTorsoMeshRenderer;
        [SerializeField] private LayerMask _playerLayerMask;
        [SerializeField] private float _sightRange;
        [SerializeField] private float _attackRange;
        [SerializeField] private GameObject _bloodSprayEffectPrefab;
        [SerializeField] private Weapon _weapon;
        [SerializeField] private Transform _camera;
        [SerializeField] private GameObject _muzzleFlash;
#pragma warning restore CS0649 

        //Non-Serialized Fields
        public float MaxHealth = 200f;
        public float CurrentHealth;
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

        [UsedImplicitly]
        private void Awake()
        {
            CurrentHealth = MaxHealth;
            _muzzleFlashEffect = _muzzleFlash.GetComponent<ParticleSystem>();
            _weaponAudio = _weapon.GetComponent<AudioSource>();
        }

        [UsedImplicitly]
        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.Find(nameof(Player)).transform;
        }

        [UsedImplicitly]
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
                _attackCoroutine ??= StartCoroutine(AttackPlayerRoutine());

            if (_weapon.BulletsRemainingInMag == 0)
                _reloadCoroutine ??= StartCoroutine(ReloadWeaponRoutine());
        }

        private IEnumerator ReloadWeaponRoutine()
        {
            while (_weapon.BulletsRemainingInMag < _weapon.MaxBulletsInMag)
            {
                _weapon.BulletsRemainingInMag++;
                yield return new WaitForSeconds(_weapon.WeaponSo.SecondsGapInReloading);
            }

            _reloadCoroutine = null;
        }

        private IEnumerator AttackPlayerRoutine()
        {
            _agent?.SetDestination(transform.position);
            _agent?.transform.LookAt(_player);
            while (_weapon.BulletsRemainingInMag > 0 && _isPlayerInAttackRange && _reloadCoroutine == null)
            {
                Attack();
                yield return new WaitForSeconds(_weapon.WeaponSo.SecondsGapBetweenBullets);
            }

            _attackCoroutine = null;
        }

        private void Attack()
        {
            _agent?.transform.LookAt(_player);
            Vector3 bulletDir = _camera.transform.forward +
            (_camera.transform.right * Random.Range(-0.02f, 0.02f)) +
            (_camera.transform.up * Random.Range(0, 0.02f));

            if (Physics.Raycast(_camera.transform.position, bulletDir, out RaycastHit hit, _attackRange))
            {
                IDamageable damagebale = hit.collider.GetComponentInParent<IDamageable>();
                _weapon.BulletsRemainingInMag--;
                _weaponAudio.PlayOneShot(_weaponAudio.clip);
                _muzzleFlashEffect.Play();
                damagebale?.Damage(hit);
                print($"Enemy bullets: {_weapon.BulletsRemainingInMag}");
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

        private void TakeDamage(int damage, MeshRenderer enemyMeshRenderer)
        {
            CurrentHealth -= damage;
            OnDamage?.Invoke();

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Destroy(gameObject);
                _agent = null;
                return;
            }

            StartCoroutine(PlayerDamagedRoutine(enemyMeshRenderer));
        }

        private IEnumerator PlayerDamagedRoutine(MeshRenderer enemyMeshRenderer)
        {
            enemyMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(_colorChangedAfterDamageSeconds);
            enemyMeshRenderer.material.color = Color.white;
        }
    }
}
