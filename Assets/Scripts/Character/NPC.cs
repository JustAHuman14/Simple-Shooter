using UnityEngine;

namespace Assets.Scripts.Character
{
    public class NPC : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundLayer;

        private Animator _animator;
        private Collider _collider;
        private Rigidbody[] _ragdollBones;
        private bool _shouldReset;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            _ragdollBones = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.isKinematic = true;
                bone.gameObject.AddComponent<Joints>();
            }
        }

        private void Update()
        {
            if (GlobalReferences.Instance.gameInput.IsPlayerResettingRagdoll())
                _shouldReset = true;
        }

        private void FixedUpdate()
        {
            if (_shouldReset)
            {
                _collider.enabled = false;

                foreach (Rigidbody bone in _ragdollBones)
                {
                    bone.isKinematic = true;
                    bone.velocity = Vector3.zero;
                    bone.angularVelocity = Vector3.zero;
                    bone.gameObject.GetComponentInChildren<Collider>().enabled = false;
                }

                transform.localRotation = Quaternion.Euler(0, -90, 0);
                transform.position = Vector3.zero;
                _animator.enabled = true;
                _animator.Update(0);
                Physics.SyncTransforms();
                _collider.enabled = true;
                _shouldReset = false;
            }
        }

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
            }

            _animator.enabled = false;

            foreach (Rigidbody bone in _ragdollBones)
            {
                bone.isKinematic = false;
            }

            _collider.enabled = false;
        }
    }
}
