using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IPickable
    {
        public void Pick(Transform weaponSlot);
        public bool IsPicked { get; }
    }
}
