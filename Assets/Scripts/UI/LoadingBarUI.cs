using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class LoadingBarUI : MonoBehaviour
    {
        [SerializeField] private Image _loadingBarForeground;
    
        private void Update()
    	{
            _loadingBarForeground.fillAmount = Mathf.Lerp(_loadingBarForeground.fillAmount, SceneChanger.Instance.sceneLoadProgress, Time.deltaTime * 10f);
	    print(_loadingBarForeground.fillAmount);	
			
            if (_loadingBarForeground.fillAmount > 0.99f)
            	
                SceneChanger.Instance.Loaded();
    	}
    }
}
