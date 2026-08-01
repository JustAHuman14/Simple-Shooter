using JetBrains.Annotations;
using UnityEngine.UI;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Serialized Fields")]
#pragma warning disable CS0649
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;
#pragma warning restore CS0649

        [UsedImplicitly]
        private void Awake()
        {
            _startButton.onClick.AddListener(() => { SceneChanger.Instance.LoadScene(1); });

            _quitButton.onClick.AddListener(() => { Application.Quit(); });
        }
    }
}
