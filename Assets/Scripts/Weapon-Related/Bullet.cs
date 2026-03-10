using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Weapon_Related
{
    public class Bullet : MonoBehaviour
    {
        public IObjectPool<Bullet> bulletObjectPool;

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint collisionContactPoint = collision.contacts[0];
            Vector3 collisionPoint = collision.contacts[0].point;

            Transform bulletImpactInstance = Instantiate(
                GlobalReferences.Instance.bulletImpactPrefab.transform,
                collisionPoint + (collisionContactPoint.normal * 0.01f),
                Quaternion.LookRotation(collisionContactPoint.normal)
            );

            bulletImpactInstance.SetParent(collision.transform);

            bulletObjectPool.Release(this);
        }
    }
}
