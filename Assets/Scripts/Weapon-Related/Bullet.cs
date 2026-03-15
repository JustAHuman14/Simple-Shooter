using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Weapon_Related
{
    public class Bullet : MonoBehaviour
    {
        public IObjectPool<Bullet> bulletObjectPool;
        private bool _isReleasingBullet;

        private void OnEnable()
        {
            StartCoroutine(ReleaseBulletToPoolRoutine());
        }

        private IEnumerator ReleaseBulletToPoolRoutine()
        {
            yield return new WaitForSeconds(3);
            _isReleasingBullet = true;
            ReleaseBulletToPool();
        }

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint collisionContactPoint = collision.contacts[0];
            Vector3 collisionPoint = collision.contacts[0].point;
           
            if (collision.gameObject.CompareTag("Wall"))
            {
                Transform bulletImpactInstance = Instantiate(
                    GlobalReferences.Instance.bulletImpactPrefab.transform,
                    collisionPoint + (collisionContactPoint.normal * 0.01f),
                    Quaternion.LookRotation(collisionContactPoint.normal)
                );
 	    
                bulletImpactInstance.SetParent(collision.transform);
            }

            if (!collision.gameObject.CompareTag("Weapon"))
            {
            	ReleaseBulletToPool();
            }
        }

        private void ReleaseBulletToPool()
        {
            if (!gameObject.activeInHierarchy || _isReleasingBullet) return;

            bulletObjectPool.Release(this);
        }
    }
}
