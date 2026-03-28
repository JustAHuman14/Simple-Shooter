using UnityEngine;

namespace Assets.Scripts.Weapon_Related
{
    public enum WeaponType
    {
        Primary,
        Secondary
    }

    [CreateAssetMenu()]
    public class WeaponSO : ScriptableObject
    {
        public WeaponType weaponType;
        public int maxBulletsInMag;
        public float secondsGapBetweenBullets;
        public float secondsGapInReloading;
        public float bulletRange;
        public Vector3 gunPosition;
        public float spreadDensityX;
        public float spreadDensityY;
    }
}
