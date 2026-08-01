using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Character
{
    // ReSharper disable once InconsistentNaming
    public class NPC : MonoBehaviour
    {
        // Serialized Fields
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private LayerMask _groundLayer;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        // Non-Serialized Fields
        private Animator _animator;
        private Rigidbody[] _ragdollBones;
        private bool _shouldReset;

        [UsedImplicitly]
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _ragdollBones = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.isKinematic = true;
                bone.gameObject.AddComponent<Bones>()
                    .OnCollision += other => TurnToRagdoll(other.collider);
            }
        }

        [UsedImplicitly]
        private void Update()
        {
            if (GlobalReferences.Instance.GameInput.IsPlayerResettingRagdoll())
                _shouldReset = true;
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            if (!_shouldReset) return;
            //  _collider.enabled = false;

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.isKinematic = true;
                bone.velocity = Vector3.zero;
                bone.angularVelocity = Vector3.zero;
                bone.gameObject.GetComponentInChildren<Collider>().enabled = false;
            }

            transform.localRotation = Quaternion.Euler(0, -90, 0);
            transform.localPosition = Vector3.zero;
            _animator.enabled = true;
            _animator.Update(0);
            Physics.SyncTransforms();
            //    _collider.enabled = true;
            _shouldReset = false;

            StartCoroutine(ColliderOnRoutine());
        }

        private IEnumerator ColliderOnRoutine()
        {
            yield return new WaitForSeconds(0.05f);

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.gameObject.GetComponentInChildren<Collider>().enabled = true;
            }
        }

        [UsedImplicitly]
        private void OnCollisionEnter(Collision other)
        {
            TurnToRagdoll(other.collider);
        }

        public void TurnToRagdoll(Collider other)
        {
            foreach (Rigidbody bone in _ragdollBones)
            {
                if (other.gameObject.layer == _groundLayer) return;
                if (other == bone.GetComponent<Collider>()) return;

                bone.gameObject.GetComponentInChildren<Collider>().enabled = true;
            }

            _animator.enabled = false;

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.isKinematic = false;
                bone.maxDepenetrationVelocity = 100f;
            }

            //  _collider.enabled = false;
        }
    }
}
