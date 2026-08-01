using UnityEngine;

namespace Assets.Scripts.Weapon_Related
{
    public enum WeaponType
    {
        Primary,
        Secondary
    }
    
    // ReSharper disable once InconsistentNaming
    [CreateAssetMenu()]
    public class WeaponSO : ScriptableObject
    {
        public WeaponType WeaponType;
        public int MaxBulletsInMag;
        public float SecondsGapBetweenBullets;
        public float SecondsGapInReloading;
        public float BulletRange;
        public Vector3 GunPosition;
        public float SpreadDensityX;
        public float SpreadDensityY;
    }
}
