using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Character;

namespace Assets.Scripts.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private Enemy _enemy;
        [SerializeField] private Image _healthBarImage;

        // Non-Serialized Fields
        private Transform _playerPOV;

        private void Awake()
        {
            _enemy.OnDamage += UpdateEnemyHealth;

            UpdateEnemyHealth();
        }

        private void Start()
        {
            _playerPOV = GameObject.Find("PlayerPOV").transform;
        }

        private void UpdateEnemyHealth()
        {
            _healthBarImage.fillAmount = _enemy.currentHealth / _enemy.maxHealth;
        }

        private void LateUpdate()
        {
            transform.LookAt(_playerPOV.position);
        }

        private void OnDestroy()
        {
            if (_enemy != null)
                _enemy.OnDamage -= UpdateEnemyHealth;
        }
    }
}
