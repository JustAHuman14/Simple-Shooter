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
    public Transform bulletSpawn;
    public AudioSource audioSource;
    public ParticleSystem muzzleFlashEffect;
    public float secondsGapBetweenBullets;
    public float secondsGapInReloading;
    public int bulletSpeed;
}
