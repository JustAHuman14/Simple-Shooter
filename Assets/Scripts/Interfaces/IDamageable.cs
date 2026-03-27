using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IDamageable
    {
        public void Damage(RaycastHit hit);
    }
}
