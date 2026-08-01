using UnityEngine;
using TMPro;
using Assets.Scripts.Character;
using Assets.Scripts.Weapon_Related;
using Assets.Scripts.Interfaces;
using JetBrains.Annotations;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    public class BulletsRemaining : MonoBehaviour
    {
        [Header("Non-Serialized Fields")]
        private Player _player;
        private TextMeshProUGUI _text;

        [UsedImplicitly]
        private void Awake()
        {
            _player = GameObject.Find(nameof(Player)).GetComponent<Player>();
            _text = GetComponent<TextMeshProUGUI>();
        }

        [UsedImplicitly]
        private void Start()
        {
            _player.OnWeaponSwitch += UpdateTotalAmmo;
            _player.OnWeaponShoot += UpdateTotalAmmo;
            _player.OnWeaponReload += UpdateTotalAmmo;
        }

        private void UpdateTotalAmmo(IWeapon iWeapon)
        {
            if (iWeapon is Weapon weapon)
            {
                _text.text = $"{weapon.BulletsRemainingInMag}/{weapon.MaxBulletsInMag}";
                weapon.OnWeaponDropped += () => _text.text = "";
            }
            else 
            	_text.text = "";
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (_player == null) return;
            _player.OnWeaponSwitch -= UpdateTotalAmmo;
            _player.OnWeaponShoot -= UpdateTotalAmmo;
            _player.OnWeaponReload -= UpdateTotalAmmo;
        }
    }
}
