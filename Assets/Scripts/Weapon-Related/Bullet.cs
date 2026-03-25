// using System;
// using System.Collections;
// using System.Diagnostics;
// using UnityEngine;
// using UnityEngine.Pool;

// namespace Assets.Scripts.Weapon_Related
// {
//     public class Bullet : MonoBehaviour
//     {

//         private void OnCollisionEnter(Collision collision)
//         {
//             ContactPoint collisionContactPoint = collision.contacts[0];
//             Vector3 collisionPoint = collision.contacts[0].point;

//             if (collision.gameObject.CompareTag("Wall"))
//             {

//             }
//             ReleaseBulletToPool();
//         }

//         private void ReleaseBulletToPool()
//         {
//             if (!gameObject.activeInHierarchy || _isReleasingBullet) return;

//             bulletObjectPool.Release(this);
//         }

//         private void OnDisable()
//         {
//             StopAllCoroutines();
//         }
//     }
// }
