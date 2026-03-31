using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class SceneChanger : MonoBehaviour
    {
        public static SceneChanger Instance { get; private set; }
	public float sceneLoadProgress;
	private bool _isLoaded;
	
        private void Awake()
        {
            if (Instance != null && Instance != this)
	    {
	        Destroy(gameObject);
	        return;
	    }    

	    Instance = this;
            DontDestroyOnLoad(gameObject);
        }
	
	public void Loaded()
	{
	    _isLoaded = true;	
	}        
	
        public void LoadScene(int sceneIndex)
        {
	    sceneLoadProgress = 0;
	    _isLoaded = false;
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        }

        private IEnumerator LoadSceneRoutine(int sceneIndex)
        {
            SceneManager.LoadScene(2);
            yield return null;
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneIndex);
            loadScene.allowSceneActivation = false;
            while (loadScene.progress < 0.9f)
            {
                Debug.Log($"Scene Loading: {loadScene.progress}");
                sceneLoadProgress = Mathf.Clamp01(loadScene.progress/0.9f);
                yield return null;
            }
	
	    sceneLoadProgress = 1;

	    while (!_isLoaded)
	    {
	    	yield return null;
	    }
	    
            loadScene.allowSceneActivation = true;
        }
    }
}

