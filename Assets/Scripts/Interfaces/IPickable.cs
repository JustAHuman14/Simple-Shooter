using UnityEngine;
using Assets.Scripts.Weapon_Related;

namespace Assets.Scripts.Interfaces
{
    public interface IPickable
    {
        public void Pick(Transform player, WeaponType weaponType);
    }
}
