using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.Character;
using Assets.Scripts.Weapon_Related;

namespace Assets.Scripts.UI
{
    public class BulletsRemaining : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] private List<GameObject> _weaponsList;
        [SerializeField] private Player _player;

        [Header("Non-Serialized Fields")]
        private TextMeshProUGUI _text;

        void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();

            foreach (GameObject weaponObject in _weaponsList)
            {
                Weapon weapon = weaponObject.GetComponent<Weapon>();
                weapon.OnShoot += UpdateTotalAmmo;
                weapon.OnReload += UpdateTotalAmmo;
            }
            _player.OnWeaponSwitch += UpdateTotalAmmo;

            UpdateTotalAmmo();
        }

        private void UpdateTotalAmmo()
        {
            foreach (GameObject weaponObject in _weaponsList)
            {
                if (weaponObject != null && weaponObject.gameObject.activeInHierarchy)
                {
                    Weapon weapon = weaponObject.GetComponent<Weapon>();
                    _text.text = $"{weapon.bulletsRemainingInMag}/{weapon.maxBulletsInMag}";
                }
            }
        }

        private void OnDestroy()
        {
            foreach (GameObject weaponObject in _weaponsList)
            {
                if (weaponObject != null)
                {
                    Weapon weapon = weaponObject.GetComponent<Weapon>();
                    weapon.OnShoot -= UpdateTotalAmmo;
                    weapon.OnReload -= UpdateTotalAmmo;
                }
            }
        }
    }
}
