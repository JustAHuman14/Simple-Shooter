using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GlobalReferences : MonoBehaviour
    {
        public GameObject bulletImpactPrefab;
        public GameInput gameInput;

        public static GlobalReferences Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
