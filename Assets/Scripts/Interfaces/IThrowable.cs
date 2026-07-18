using System;

namespace Assets.Scripts.Interfaces
{
    public interface IThrowable : IWeapon
    {
        public void Throw();
        public event Action OnThrow;
    }
}
