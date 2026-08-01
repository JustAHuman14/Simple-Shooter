using UnityEngine;
using System;
using JetBrains.Annotations;

namespace Assets.Scripts.Character
{
    public class Bones : MonoBehaviour
    {
        public event Action<Collision> OnCollision;

        [UsedImplicitly]
        private void OnCollisionEnter(Collision other)
        {
            OnCollision?.Invoke(other);    
        }
    }
}