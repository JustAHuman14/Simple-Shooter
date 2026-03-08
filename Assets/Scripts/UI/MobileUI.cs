using UnityEngine;

public class MobileUI : MonoBehaviour
{
    private void Update()
    {
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
