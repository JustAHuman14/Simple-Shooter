using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public float MouseSensitivity;
        public static GameManager Instance { get; private set; }

        [UsedImplicitly]
        private void Awake()
        {
            MouseSensitivity = 10f;
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
