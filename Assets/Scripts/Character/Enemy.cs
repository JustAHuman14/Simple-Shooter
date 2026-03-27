using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.AI;

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

        //Non-Serialized Fields
        public float maxHealth = 200f;
        public float currentHealth;
        public event Action OnDamage;
        private NavMeshAgent _agent;
        private bool _isPlayerInSightRange;
        private bool _isPlayerInAttackRange;
        private Transform _player;
        private float _fieldOfView = 60f;
        private readonly float _colorChangedAfterDamageSeconds = 0.05f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.Find(nameof(Player)).transform;
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, _player.position) <= _sightRange)
            {
                if (Vector3.Angle(transform.position, _player.position) >= -_fieldOfView && Vector3.Angle(transform.position, _player.position) <= _fieldOfView)
                    _isPlayerInSightRange = true;
            }

            if (Vector3.Distance(_player.transform.position, transform.position) <= _attackRange)
            {
                if (Vector3.Angle(transform.position, _player.position) >= -_fieldOfView && Vector3.Angle(transform.position, _player.position) <= _fieldOfView)
                    _isPlayerInAttackRange = true;
            }

            if (_isPlayerInSightRange && !_isPlayerInAttackRange)
                ChasePlayer();
            else if (_isPlayerInAttackRange)
                AttackPlayer();
        }

        private void AttackPlayer()
        {
            _agent.SetDestination(transform.position);
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
                _agent = null;
                Destroy(gameObject);
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
