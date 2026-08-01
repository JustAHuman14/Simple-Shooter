using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Character;
using JetBrains.Annotations;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    public class EnemyHealthBar : MonoBehaviour
    {
        // Serialized Fields
#pragma warning disable CS0649
        [SerializeField] private Enemy _enemy;
        [SerializeField] private Image _healthBarImage;
#pragma warning restore CS0649

        // Non-Serialized Fields
        private GameObject[] _playerPovArray;

        [UsedImplicitly]
        private void Awake() => _playerPovArray = GameObject.FindGameObjectsWithTag("PlayerPOV");

        [UsedImplicitly]
        private void Start()
        {
            _enemy.OnDamage += UpdateEnemyHealth;
            UpdateEnemyHealth();
        }

        private void UpdateEnemyHealth()
        {
            _healthBarImage.fillAmount = _enemy.CurrentHealth / _enemy.MaxHealth;
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            foreach (GameObject playerPov in _playerPovArray)
            {
                transform.LookAt(playerPov.transform);
            }
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (_enemy != null)
                _enemy.OnDamage -= UpdateEnemyHealth;
        }
    }
}
