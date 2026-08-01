using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public class LoadingBarUI : MonoBehaviour
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
	    [SerializeField] private Image _loadingBarForeground;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    
        [UsedImplicitly]
	    private void Update()
    	{
            _loadingBarForeground.fillAmount = Mathf.Lerp(_loadingBarForeground.fillAmount, SceneChanger.Instance.SceneLoadProgress, Time.deltaTime * 10f);
			
            if (_loadingBarForeground.fillAmount > 0.99f)
            	
                SceneChanger.Instance.Loaded();
    	}
    }
}
