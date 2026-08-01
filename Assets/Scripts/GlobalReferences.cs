using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    public class GlobalReferences : MonoBehaviour
    {
        public GameObject BulletImpactPrefab;
        public GameInput GameInput;
        public GameObject PickupUi;
        public GameObject PauseMenuUi;

        public static GlobalReferences Instance { get; private set; }

        [UsedImplicitly]
        private void Awake()
        {
            Instance = this;
        }
    }
}
