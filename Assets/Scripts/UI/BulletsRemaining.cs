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

        [Header("Non-Serialized Fields")]
        private Player _player;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _player = GameObject.Find(nameof(Player)).GetComponent<Player>();
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
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
            if (_player != null)
            {
                _player.OnWeaponSwitch -= UpdateTotalAmmo;
                _player.OnWeaponShoot -= UpdateTotalAmmo;
                _player.OnWeaponReload -= UpdateTotalAmmo;
            }
        }
    }
}
