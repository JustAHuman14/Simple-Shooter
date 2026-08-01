using System;
using System.Collections;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using JetBrains.Annotations;

namespace Assets.Scripts.Weapon_Related
{
    public class Grenade : MonoBehaviour, IThrowable, IPickable
    {
        //Serialized Field
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private float _throwForce;
        [SerializeField] private float _upwardsModifier;
        [SerializeField] private float _blastRadius;
        [SerializeField] private float _explosionForce;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private GameObject _grenadeGameObject;
        [SerializeField] private ParticleSystem _explosionEffect;
        [SerializeField] private GameObject _pin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        //Non-Serialized Field
        private Rigidbody _rb;
        private Collider _collider;
        private Rigidbody _playerRb;
        private Camera _playerCamera;
        private Transform[] _children;
        private bool _isTicking;
        private bool _isThrowing;
        public bool IsPicked { get; private set; }
        public event Action OnThrow;

        [UsedImplicitly]
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _playerRb = GameObject.Find(nameof(Player)).GetComponent<Rigidbody>();
            _children = GetComponentsInChildren<Transform>();
        }

        [UsedImplicitly]
        private void Update()
        {
            if (Time.timeScale != 0 && IsPicked && !_isTicking &&
                GlobalReferences.Instance.GameInput.IsPlayerAttacking())
                _isThrowing = true;
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            if (_isThrowing)
            {
                Throw();
                _isThrowing = false;
            }
        }

        public void Throw()
        {
            IsPicked = false;
            OnThrow?.Invoke();
            _isTicking = true;
            Destroy(_pin);
            transform.parent = null;
            _rb.isKinematic = false;
            _collider.isTrigger = false;
            _rb.velocity = _playerRb.velocity;
            _rb.AddForce(_playerCamera.transform.forward * _throwForce, ForceMode.Impulse);

            foreach (Transform child in _children)
            {
                child.gameObject.layer = 0;
            }

            StartCoroutine(GrenadeBlastRoutine());
        }

        private IEnumerator GrenadeBlastRoutine()
        {
            yield return new WaitForSeconds(3.8f);

            _explosionEffect.GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(0.2f);

            _explosionEffect.transform.parent = null;
            _explosionEffect.transform.position = transform.position;

            Destroy(_grenadeGameObject);

            Collider[] colliders = Physics.OverlapSphere(transform.position, _blastRadius, _enemyLayerMask);
            _explosionEffect.transform.localScale = new(4.7f, 4.7f, 4.7f);
            _explosionEffect.transform.rotation = Quaternion.Euler(0, 0, 0);
            _explosionEffect.Play();

            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out Rigidbody rb))
                {
                    if (rb != _rb)
                    {
                        rb.GetComponentInParent<NPC>()?.TurnToRagdoll(_collider);
                        rb.AddExplosionForce(_explosionForce, transform.position, _blastRadius, _upwardsModifier,
                            ForceMode.Impulse);
                    }
                }
            }

            yield return new WaitForSeconds(2f);
            Destroy(_explosionEffect.gameObject);
            Destroy(gameObject);
        }

        public void Pick(Transform weaponSlot)
        {
            if (IsPicked) return;

            transform.SetParent(weaponSlot);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            _grenadeGameObject.transform.localPosition = Vector3.zero;
            _grenadeGameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            IsPicked = true;
            _rb.isKinematic = true;
            _collider.isTrigger = true;
            _playerCamera = GetComponentInParent<Player>().GetComponentInChildren<Camera>();

            foreach (Transform child in _children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }
    }
}
