using UnityEngine;

[CreateAssetMenu()]
public class WeaponSO : ScriptableObject
{
    public enum WeaponType
    {
        Primary = 0,
        Secondary = 1
    }

    public WeaponType weaponType;
    public int maxBulletsInMag;
    public float secondsGapBetweenBullets;
    public float secondsGapInReloading;
    public int bulletSpeed;
}
