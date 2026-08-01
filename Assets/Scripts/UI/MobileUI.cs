using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public class MobileUI : MonoBehaviour
    {
        [UsedImplicitly]
        private void Update()
        {
            gameObject.SetActive(Application.isMobilePlatform);
        }
    }
}