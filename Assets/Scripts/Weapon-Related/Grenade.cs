using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Character;
using System;
using Unity.Mathematics;

namespace Assets.Scripts.Weapon_Related
{
    public class Grenade : MonoBehaviour, IThrowable, IPickable
    {
        //Serialized Field
        [SerializeField] private float _throwForce;
        [SerializeField] private float _upwardsModifier;
        [SerializeField] private float _blastRadius;
        [SerializeField] private float _explosionForce;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private ParticleSystem _explosionEffect;

        //Non-Serialized Field
        private Rigidbody _rb;
        private Collider _collider;
        private Rigidbody _playerRb;
        private Camera _playerCamera;
        private Transform[] _children;
        private bool _isTicking;
        public bool IsPicked { get; private set; }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _playerRb = GameObject.Find(nameof(Player)).GetComponent<Rigidbody>();
            _children = GetComponentsInChildren<Transform>();
        }

        private void Update()
        {
            if (IsPicked && GlobalReferences.Instance.gameInput.IsPlayerAttacking())
                Throw();
        }

        private void OnDrawGizmos()
        {
            // Gizmos.DrawSphere(transform.position, _blastRadius);
        }

        public void Throw()
        {
            IsPicked = false;
            _isTicking = !_isTicking;
            transform.parent = null;
            _rb.isKinematic = false;
            _collider.isTrigger = false;
            _rb.velocity = _playerRb.velocity;
            _rb.AddForce(_playerCamera.transform.forward * _throwForce, ForceMode.Impulse);

            foreach (Transform child in _children)
            {
                child.gameObject.layer = 0;
            }

            if (_isTicking)
                StartCoroutine(GrenadeBlastRoutine());
        }

        private IEnumerator GrenadeBlastRoutine()
        {
            yield return new WaitForSeconds(3.6f);
            Debug.Log("Boom!");
            Collider[] colliders = Physics.OverlapSphere(transform.position, _blastRadius, _enemyLayerMask);

            _explosionEffect = Instantiate(GlobalReferences.Instance.explosionEffect, transform.position, Quaternion.identity);
	    _explosionEffect.GetComponent<AudioSource>().Play();
	    yield return new WaitForSeconds(0.4f);
            _explosionEffect.Play();

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out Rigidbody rigidbody))
                {
                    if (rigidbody != _rb)
                        rigidbody.AddExplosionForce(_explosionForce, transform.position, _blastRadius, _upwardsModifier, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(0.9f);
            Destroy(_explosionEffect);
            Destroy(gameObject);
        }

        public void Pick(Transform weaponSlot)
        {
            if (IsPicked) return;

            transform.SetParent(weaponSlot);
            transform.localPosition = Vector3.zero;
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
    }
}
