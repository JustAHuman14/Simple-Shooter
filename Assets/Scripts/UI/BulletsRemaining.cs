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
            _player.OnWeaponSwitch += UpdateTotalAmmo;
            _player.OnWeaponShoot += UpdateTotalAmmo;
            _player.OnWeaponReload += UpdateTotalAmmo;
        }

        private void UpdateTotalAmmo(Weapon weapon)
        {
            _text.text = $"{weapon.bulletsRemainingInMag}/{weapon.maxBulletsInMag}";
        }

        private void OnDestroy()
        {
            _player.OnWeaponSwitch -= UpdateTotalAmmo;
        }
    }
}
