using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Character
{
    public class Enemy : MonoBehaviour
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
        public event Action OnDeath;
        private NavMeshAgent _agent;
        private bool _isPlayerInSightRange;
        private bool _isPlayerInAttackRange;
        private Transform _player;
        private readonly float _colorChangedAfterDamageSeconds = 0.05f;

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        private void Start() => currentHealth = maxHealth;

        private void Update()
        {
            HandleDamage();

            _player = GameObject.Find(nameof(Player)).transform;

            _isPlayerInSightRange = Physics.CheckSphere(transform.position, _sightRange, _playerLayerMask);
            _isPlayerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, _playerLayerMask);

            if (_isPlayerInSightRange && !_isPlayerInAttackRange)
                ChasePlayer();
            else if (_isPlayerInAttackRange && _isPlayerInAttackRange)
                AttackPlayer();
        }

        private void AttackPlayer()
        {
            _agent.SetDestination(transform.position);
        }

        private void ChasePlayer() => _agent?.SetDestination(_player.position);

        private void HandleDamage()
        {
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke();
                Destroy(gameObject);
            }

            OnDamage?.Invoke();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "Bullet(Clone)")
            {
                string bodyPartCollided = collision.contacts[0].thisCollider.name;

                switch (bodyPartCollided)
                {
                    case "EnemyHead":
                        TakeDamage(20, _enemyHeadMeshRenderer);
                        break;
                    case "EnemyTorso":
                        TakeDamage(10, _enemyTorsoMeshRenderer);
                        break;
                }
            }
        }

        private void TakeDamage(int damage, MeshRenderer _enemyMeshRenderer)
        {
            currentHealth -= damage;
            StartCoroutine(PlayerDamagedRoutine(_enemyMeshRenderer));
        }

        private IEnumerator PlayerDamagedRoutine(MeshRenderer _enemyMeshRenderer)
        {
            _enemyMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(_colorChangedAfterDamageSeconds);
            _enemyMeshRenderer.material.color = Color.white;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, _sightRange);
            Gizmos.color = Color.yellow;
        }
    }
}
